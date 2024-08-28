using Microsoft.AspNetCore.Mvc;
using POEtest.Models;
using POEtest.Services;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace POEtest.Controllers
{
    public class OrdersController : Controller
    {
        private readonly TableStorageService _tableStorageService;
        private readonly QueueService _queueService;

        public OrdersController(TableStorageService tableStorageService, QueueService queueService)
        {
            _tableStorageService = tableStorageService;
            _queueService = queueService;
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
            var products = await _tableStorageService.GetAllProductsAsync(); // Corrected method call

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
                order.Order_Date = DateTime.SpecifyKind(order.Order_Date, DateTimeKind.Utc);
                order.PartitionKey = "OrdersPartition";
                order.RowKey = Guid.NewGuid().ToString();
                await _tableStorageService.AddOrderAsync(order);

                string message = $"New order from user {order.User_ID} of product {order.Product_ID} at {order.Order_Location} on {order.Order_Date}";
                await _queueService.SendMessageAsync(message);

                return RedirectToAction("Index");
            }

            // If validation fails, reload the users and products
            var users = await _tableStorageService.GetAllUsersAsync();
            var products = await _tableStorageService.GetAllProductsAsync();
            ViewData["Users"] = users;
            ViewData["Products"] = products;

            return View(order);
        }
    }
}

