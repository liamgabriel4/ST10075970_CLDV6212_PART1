﻿using Microsoft.AspNetCore.Mvc;
using POEtest.Models;
using POEtest.Services;
using System.Threading.Tasks;

namespace POEtest.Controllers
{
    public class UsersController : Controller
    {
        private readonly TableStorageService _tableStorageService;

        public UsersController(TableStorageService tableStorageService)
        {
            _tableStorageService = tableStorageService;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _tableStorageService.GetAllUsersAsync();
            return View(users);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(User user)
        {
            user.PartitionKey = "UsersPartition"; // Set the partition key
            user.RowKey = Guid.NewGuid().ToString(); // Generate a unique row key

            await _tableStorageService.AddUserAsync(user);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            await _tableStorageService.DeleteUserAsync(partitionKey, rowKey);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Details(string partitionKey, string rowKey)
        {
            var user = await _tableStorageService.GetUserAsync(partitionKey, rowKey);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }
    }
    //Mrzygłód, K., 2022. Azure for Developers. 2nd ed. August: [Meeta Rajani]
}