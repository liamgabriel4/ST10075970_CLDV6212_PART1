using Azure;
using Azure.Data.Tables;
using POEtest.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace POEtest.Services
{
    public class TableStorageService
    {
        private readonly TableClient _productTableClient;
        private readonly TableClient _userTableClient;
        private readonly TableClient _orderTableClient;

        public TableStorageService(string connectionString)
        {
            _productTableClient = new TableClient(connectionString, "Products");
            _userTableClient = new TableClient(connectionString, "Users");
            _orderTableClient = new TableClient(connectionString, "Orders");
        }

        // Product Methods
        public async Task<List<Product>> GetAllProductsAsync()
        {
            var products = new List<Product>();

            await foreach (var product in _productTableClient.QueryAsync<Product>())
            {
                products.Add(product);
            }

            return products;
        }

        public async Task AddProductAsync(Product product)
        {
            if (string.IsNullOrEmpty(product.PartitionKey) || string.IsNullOrEmpty(product.RowKey))
            {
                throw new ArgumentException("PartitionKey and RowKey must be set.");
            }

            try
            {
                await _productTableClient.AddEntityAsync(product);
            }
            catch (RequestFailedException ex)
            {
                throw new InvalidOperationException("Error adding product to Table Storage", ex);
            }
        }

        public async Task DeleteProductAsync(string partitionKey, string rowKey)
        {
            await _productTableClient.DeleteEntityAsync(partitionKey, rowKey);
        }

        public async Task<Product?> GetProductAsync(string partitionKey, string rowKey)
        {
            try
            {
                var response = await _productTableClient.GetEntityAsync<Product>(partitionKey, rowKey);
                return response.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }

        // Order Methods
        public async Task<List<Order>> GetAllOrdersAsync()
        {
            var orders = new List<Order>();

            await foreach (var order in _orderTableClient.QueryAsync<Order>())
            {
                orders.Add(order);
            }

            return orders;
        }

        public async Task AddOrderAsync(Order order)
        {
            if (string.IsNullOrEmpty(order.PartitionKey) || string.IsNullOrEmpty(order.RowKey))
            {
                throw new ArgumentException("PartitionKey and RowKey must be set.");
            }

            try
            {
                await _orderTableClient.AddEntityAsync(order);
            }
            catch (RequestFailedException ex)
            {
                throw new InvalidOperationException("Error adding order to Table Storage", ex);
            }
        }

        public async Task<Order?> GetOrderAsync(string partitionKey, string rowKey)
        {
            try
            {
                var response = await _orderTableClient.GetEntityAsync<Order>(partitionKey, rowKey);
                return response.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }

        public async Task DeleteOrderAsync(string partitionKey, string rowKey)
        {
            await _orderTableClient.DeleteEntityAsync(partitionKey, rowKey);
        }

        // User Methods
        public async Task<List<User>> GetAllUsersAsync()
        {
            var users = new List<User>();

            await foreach (var user in _userTableClient.QueryAsync<User>())
            {
                users.Add(user);
            }

            return users;
        }

        public async Task AddUserAsync(User user)
        {
            if (string.IsNullOrEmpty(user.PartitionKey) || string.IsNullOrEmpty(user.RowKey))
            {
                throw new ArgumentException("PartitionKey and RowKey must be set.");
            }

            try
            {
                await _userTableClient.AddEntityAsync(user);
            }
            catch (RequestFailedException ex)
            {
                throw new InvalidOperationException("Error adding user to Table Storage", ex);
            }
        }

        public async Task DeleteUserAsync(string partitionKey, string rowKey)
        {
            await _userTableClient.DeleteEntityAsync(partitionKey, rowKey);
        }

        public async Task<User?> GetUserAsync(string partitionKey, string rowKey)
        {
            try
            {
                var response = await _userTableClient.GetEntityAsync<User>(partitionKey, rowKey);
                return response.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }
    }
    //Mrzygłód, K., 2022. Azure for Developers. 2nd ed. August: [Meeta Rajani]
}