using Microsoft.AspNetCore.Mvc;
using POEtest.Models;
using POEtest.Services;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace POEtest.Controllers
{
    public class OrdersController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly TableStorageService _tableStorageService;
        private readonly string _azureFunctionBaseUrl;

        public OrdersController(HttpClient httpClient, TableStorageService tableStorageService)
        {
            _httpClient = httpClient;
            _tableStorageService = tableStorageService;

            // Set the base URL for the Azure Function
            _azureFunctionBaseUrl = "http://st10075970cloudpart2.azurewebsites.net/api"; 
        }

        // Action to display all orders
        public async Task<IActionResult> Index()
        {
            var orders = await _tableStorageService.GetAllOrdersAsync();
            return View(orders);
        }

        // GET: Register new order
        public async Task<IActionResult> Register()
        {
            var users = await _tableStorageService.GetAllUsersAsync();
            var products = await _tableStorageService.GetAllProductsAsync(); 

            // Check for null or empty lists
            if (users == null || users.Count == 0)
            {
                ModelState.AddModelError("", "No users found. Please add users first.");
                return View();
            }

            if (products == null || products.Count == 0)
            {
                ModelState.AddModelError("", "No products found. Please add products first.");
                return View();
            }

            ViewData["Users"] = users;
            ViewData["Products"] = products;

            return View();
        }

        // POST: Register new order
        [HttpPost]
        public async Task<IActionResult> Register(Order order)
        {
            if (ModelState.IsValid)
            {
                // Prepare order data
                order.Order_Date = DateTime.SpecifyKind(order.Order_Date, DateTimeKind.Utc);
                order.PartitionKey = "OrdersPartition";
                order.RowKey = Guid.NewGuid().ToString();

                // Call Azure Function to add the order
                var functionUrl = $"{_azureFunctionBaseUrl}/AddOrder";
                var orderJson = JsonSerializer.Serialize(order);
                var content = new StringContent(orderJson, Encoding.UTF8, "application/json");

                // Send HTTP POST request to Azure Function
                var response = await _httpClient.PostAsync(functionUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    // If success, redirect to the Index action
                    return RedirectToAction("Index");
                }
                else
                {
                    // Log error or display appropriate message
                    ModelState.AddModelError("", "Failed to register order.");
                }
            }

            // If validation fails, reload the users and products
            var users = await _tableStorageService.GetAllUsersAsync();
            var products = await _tableStorageService.GetAllProductsAsync();
            ViewData["Users"] = users;
            ViewData["Products"] = products;

            return View(order);
        }
    }
    //Mrzygłód, K., 2022. Azure for Developers. 2nd ed. August: [Meeta Rajani]
}