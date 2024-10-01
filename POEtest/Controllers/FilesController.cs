using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using POEtest.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

public class FilesController : Controller
{
    private readonly HttpClient _httpClient;

    public FilesController(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IActionResult> Index()
    {
        List<FileModel> files;
        try
        {
            // Call the Azure Function to list files
            files = await _httpClient.GetFromJsonAsync<List<FileModel>>("http://st10075970cloudpart2.azurewebsites.net/api/ListFiles");
        }
        catch (Exception ex)
        {
            ViewBag.Message = $"Failed to load files: {ex.Message}";
            files = new List<FileModel>();
        }

        return View(files);
    }

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            ModelState.AddModelError("file", "Please select a file to upload.");
            return await Index();
        }

        try
        {
            using (var stream = file.OpenReadStream())
            {
                // Prepare the content for the HTTP POST request
                var content = new MultipartFormDataContent();
                content.Add(new StreamContent(stream), "file", file.FileName);

                // Call the Azure Function to upload the file
                var response = await _httpClient.PostAsync("http://st10075970cloudpart2.azurewebsites.net/api/UploadFile", content);
                response.EnsureSuccessStatusCode();

                TempData["Message"] = $"File '{file.FileName}' uploaded successfully!";
            }
        }
        catch (Exception ex)
        {
            TempData["Message"] = $"File upload failed: {ex.Message}";
        }

        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> DownloadFile(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return BadRequest("File name cannot be null or empty.");
        }

        try
        {
            // Call the Azure Function to download the file
            var response = await _httpClient.GetAsync($"http://st10075970cloudpart2.azurewebsites.net/api/DownloadFile{fileName}");

            if (!response.IsSuccessStatusCode)
            {
                return NotFound($"File '{fileName}' not found.");
            }

            var stream = await response.Content.ReadAsStreamAsync();
            return File(stream, "application/octet-stream", fileName);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error downloading file: {ex.Message}");
        }
    }
    //Mrzygłód, K., 2022. Azure for Developers. 2nd ed. August: [Meeta Rajani]
}
