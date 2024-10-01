using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Azure.Data.Tables;
using Azure.Storage.Queues;
using Azure;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using System;
using System.IO;
using POEtestFunction.Models;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;


public class OrderStorage
{
    private readonly TableClient _orderTableClient;
    private readonly QueueClient _queueClient;

    public OrderStorage()
    {
        // Connection string for the storage account
        string connectionString = "DefaultEndpointsProtocol=https;AccountName=st10075970;AccountKey=FlcmGMSuBSX9CQ32dU6EUtPBlbmctKJJ9kMsqvwdb1sxI/Y85Fz2y94CduA2ebJnbUWvI09P/QaE+AStR4g4jA==;EndpointSuffix=core.windows.net";

        _orderTableClient = new TableClient(connectionString, "Orders");
        _queueClient = new QueueClient(connectionString, "orders");
    }

    [Function("OrderStorage")]
    public async Task<HttpResponseData> AddOrder(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData req,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger("OrderStorage");
        logger.LogInformation("Processing a new order.");

        // Read and deserialize the request body to an Order object
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        Order? order = JsonSerializer.Deserialize<Order>(requestBody);

        if (order == null)
        {
            var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequestResponse.WriteStringAsync("Invalid order data.");
            return badRequestResponse;
        }

        // Set the PartitionKey and RowKey if not set
        order.PartitionKey = order.Customer_ID.ToString();
        order.RowKey = Guid.NewGuid().ToString();

        try
        {
            // Insert the order entity into Azure Table Storage
            await _orderTableClient.AddEntityAsync(order);

            // Prepare the queue message
            string queueMessage = $"New order created for Customer {order.CustomerName}, Product {order.ProductName}";

            // Ensure the queue exists, and send the message to the queue
            if (!_queueClient.Exists())
            {
                await _queueClient.CreateAsync();
            }
            await _queueClient.SendMessageAsync(queueMessage);
        }
        catch (Exception ex)
        {
            logger.LogError($"Error processing order: {ex.Message}");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync("Error processing the order.");
            return errorResponse;
        }

        // Return success response
        var response = req.CreateResponse(HttpStatusCode.Created);
        await response.WriteStringAsync("Order added successfully and message sent to queue.");
        return response;
    }
}

namespace POEtestFunction.Models
{
    public class Order : ITableEntity
    {
        public int Order_Id { get; set; }
        public string? PartitionKey { get; set; }
        public string? RowKey { get; set; }

        [JsonIgnore]
        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }

        public int Customer_ID { get; set; }
        public int Product_ID { get; set; }
        public DateTime Order_Date { get; set; }
        public string? Order_Address { get; set; }
        public string? CustomerName { get; set; }
        public string? ProductName { get; set; }
    }
    //Mrzyg?ód, K., 2022. Azure for Developers. 2nd ed. August: [Meeta Rajani]
}