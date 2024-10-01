using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using POEtest.Models;
using POEtest.Services;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace POEtest.Controllers
{
    public class ProductsController : Controller
    {
        private readonly BlobService _blobService;
        private readonly TableStorageService _tableStorageService;

        public ProductsController(BlobService blobService, TableStorageService tableStorageService)
        {
            _blobService = blobService;
            _tableStorageService = tableStorageService;
        }

        [HttpGet]
        public IActionResult AddProduct()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct(Product product, IFormFile file)
        {
            if (file != null)
            {
                using var stream = file.OpenReadStream();

                // Call Azure Function to add product
                var httpClient = new HttpClient();
                var formContent = new MultipartFormDataContent
                {
                    { new StreamContent(stream), "file", file.FileName }
                };

                var jsonProduct = JsonConvert.SerializeObject(product);
                formContent.Add(new StringContent(jsonProduct, Encoding.UTF8, "application/json"), "product");

                var response = await httpClient.PostAsync("http://st10075970cloudpart2.azurewebsites.net/api/AddProduct", formContent);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Product added successfully!";
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Failed to add product.");
                }
            }

            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProduct(string partitionKey, string rowKey, string imageUrl)
        {
            if (!string.IsNullOrEmpty(imageUrl))
            {
                await _blobService.DeleteBlobAsync(imageUrl);
            }

            // Delete Table entity
            await _tableStorageService.DeleteProductAsync(partitionKey, rowKey);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Index()
        {
            var products = await _tableStorageService.GetAllProductsAsync();
            return View(products);
        }
    }
    //Mrzygłód, K., 2022. Azure for Developers. 2nd ed. August: [Meeta Rajani]
}
