﻿using Microsoft.AspNetCore.Mvc;
using POEtest.Models;
using POEtest.Services;

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
            var imageUrl = await _blobService.UploadAsync(stream, file.FileName);
            product.ImageUrl = imageUrl;
        }

        if (ModelState.IsValid)
        {
            product.PartitionKey = "ProductsPartition";
            product.RowKey = Guid.NewGuid().ToString();
            await _tableStorageService.AddProductAsync(product);
            return RedirectToAction("Index");
        }
        return View(product);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteProduct(string partitionKey, string rowKey, Product product )
    {
         
        if (product != null && !string.IsNullOrEmpty(product.ImageUrl))
        {
            // Delete the associated blob image
            await _blobService.DeleteBlobAsync(product.ImageUrl);
        }
        //Delete Table entity
        await _tableStorageService.DeleteProductAsync(partitionKey, rowKey);

        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Index()
    {
        var products = await _tableStorageService.GetAllProductsAsync();
        return View(products);
    }
    }
}
