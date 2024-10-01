using System.Net.Http;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace POEtest.Services
{
    public class BlobService
    {
        private readonly HttpClient _httpClient;
        private readonly string _functionBaseUrl;

        public BlobService(IConfiguration configuration, HttpClient httpClient)
        {
            _httpClient = httpClient;
            // Retrieve the base URL from configuration
            _functionBaseUrl = configuration["http://st10075970cloudpart2.azurewebsites.net/api"]; 
        }

        // Call Azure Function to upload the blob
        public async Task<string> UploadAsync(Stream fileStream, string fileName)
        {
            var formContent = new MultipartFormDataContent();
            formContent.Add(new StreamContent(fileStream), "file", fileName);

            var response = await _httpClient.PostAsync($"{_functionBaseUrl}/UploadBlob", formContent);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                return result; // Returns Blob URI as string
            }

            throw new Exception("Blob upload failed.");
        }

        // Call Azure Function to delete the blob
        public async Task DeleteBlobAsync(string blobUri)
        {
            Uri uri = new Uri(blobUri);
            string blobName = uri.Segments[^1]; // Extract blob file name from URI
            var response = await _httpClient.DeleteAsync($"{_functionBaseUrl}/DeleteBlob/{blobName}");
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Blob deletion failed.");
            }
        }
    }
    //Mrzygłód, K., 2022. Azure for Developers. 2nd ed. August: [Meeta Rajani]
}