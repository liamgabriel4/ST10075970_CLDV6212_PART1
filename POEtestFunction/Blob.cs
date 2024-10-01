using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Azure.Storage.Blobs;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using HttpMultipartParser;
using System;
using MyFunctionApp.Models;

public class BlobFunction
{
    private readonly BlobServiceClient _blobServiceClient;
    private const string ContainerName = "products"; 

    public BlobFunction()
    {
        string connectionString = "DefaultEndpointsProtocol=https;AccountName=st10075970;AccountKey=FlcmGMSuBSX9CQ32dU6EUtPBlbmctKJJ9kMsqvwdb1sxI/Y85Fz2y94CduA2ebJnbUWvI09P/QaE+AStR4g4jA==;EndpointSuffix=core.windows.net";
        _blobServiceClient = new BlobServiceClient(connectionString);
    }

    [Function("AddProduct")]
    public async Task<HttpResponseData> AddProduct(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData req,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger("BlobFunction");
        logger.LogInformation("Processing file upload and product data.");

        // Parse the multipart form data
        MultipartFormDataParser formData;
        try
        {
            formData = await MultipartFormDataParser.ParseAsync(req.Body);
        }
        catch (Exception ex)
        {
            logger.LogError($"Error parsing multipart form data: {ex.Message}");
            var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequestResponse.WriteStringAsync("Invalid form data.");
            return badRequestResponse;
        }

        // Get the product JSON from the form data
        var productJson = formData.GetParameterValue("product");
        Product? product = JsonSerializer.Deserialize<Product>(productJson);

        if (product == null)
        {
            var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequestResponse.WriteStringAsync("Invalid product data.");
            return badRequestResponse;
        }

        // Upload image to Blob Storage if provided
        if (formData.Files.Count > 0)
        {
            var imageFile = formData.Files.First();
            var imageUrl = await UploadImageToBlob(imageFile);
            product.ImageUrl = imageUrl; // Store the image URL in the product
        }

        // Return success response
        var response = req.CreateResponse(HttpStatusCode.Created);
        await response.WriteStringAsync("Product added successfully and file uploaded.");
        return response;
    }

    private async Task<string> UploadImageToBlob(FilePart file)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);

        // Ensure the container exists
        await containerClient.CreateIfNotExistsAsync();

        var blobClient = containerClient.GetBlobClient($"uploads/{file.FileName}");

        // Upload the image to Blob Storage
        using (var stream = file.Data)
        {
            await blobClient.UploadAsync(stream, overwrite: true);
        }

        return blobClient.Uri.ToString();
    }
}

namespace MyFunctionApp.Models
{
    public class Product
    {
        public string Product_Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
    }
    //Mrzyg?ód, K., 2022. Azure for Developers. 2nd ed. August: [Meeta Rajani]
}
