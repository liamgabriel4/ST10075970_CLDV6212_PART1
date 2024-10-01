using Azure;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using POEtestFunction.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace POEtestFunction
{
    public class Tablestorage
    {
        private readonly ILogger<Tablestorage> _logger;
        private readonly string _connectionString = "DefaultEndpointsProtocol=https;AccountName=st10075970;AccountKey=FlcmGMSuBSX9CQ32dU6EUtPBlbmctKJJ9kMsqvwdb1sxI/Y85Fz2y94CduA2ebJnbUWvI09P/QaE+AStR4g4jA==;EndpointSuffix=core.windows.net";
        private readonly TableClient _userTableClient;

        public Tablestorage(ILogger<Tablestorage> logger)
        {
            _logger = logger;
            _userTableClient = new TableClient(_connectionString, "Users");
        }

        [Function("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
        {
            _logger.LogInformation("Getting all users.");
            var users = new List<User>();

            await foreach (var user in _userTableClient.QueryAsync<User>())
            {
                users.Add(user);
            }

            return new OkObjectResult(users);
        }

        [Function("CreateUser")]
        public async Task<IActionResult> CreateUser([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            _logger.LogInformation("Creating a new user.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var user = System.Text.Json.JsonSerializer.Deserialize<User>(requestBody);

            if (user == null)
            {
                return new BadRequestObjectResult("Invalid user data.");
            }

            user.PartitionKey = "UsersPartition";
            user.RowKey = Guid.NewGuid().ToString();

            await _userTableClient.AddEntityAsync(user);
            return new CreatedResult($"/users/{user.RowKey}", user);
        }

        [Function("GetUser")]
        public async Task<IActionResult> GetUser(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "users/{partitionKey}/{rowKey}")] HttpRequest req,
            string partitionKey, string rowKey)
        {
            _logger.LogInformation($"Getting user with PartitionKey: {partitionKey} and RowKey: {rowKey}");
            try
            {
                var response = await _userTableClient.GetEntityAsync<User>(partitionKey, rowKey);
                return new OkObjectResult(response.Value);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return new NotFoundResult();
            }
        }

        [Function("DeleteUser")]
        public async Task<IActionResult> DeleteUser(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "users/{partitionKey}/{rowKey}")] HttpRequest req,
            string partitionKey, string rowKey)
        {
            _logger.LogInformation($"Deleting user with PartitionKey: {partitionKey} and RowKey: {rowKey}");
            await _userTableClient.DeleteEntityAsync(partitionKey, rowKey);
            return new NoContentResult();
        }
    }
}

namespace POEtestFunction.Models
{
    public class User : ITableEntity
    {
        public string? PartitionKey { get; set; }
        public string? RowKey { get; set; }

        [JsonIgnore]
        public ETag ETag { get; set; }

        public DateTimeOffset? Timestamp { get; set; }

        public string Username { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }

    }
    //Mrzyg?ód, K., 2022. Azure for Developers. 2nd ed. August: [Meeta Rajani]
}