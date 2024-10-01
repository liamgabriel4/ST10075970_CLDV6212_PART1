using Azure;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using POEtestFunction.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace POEtestFunction
{
    public class FileFunctions
    {
        private readonly ILogger<FileFunctions> _logger;

        private readonly string _connectionString = "DefaultEndpointsProtocol=https;AccountName=st10075970;AccountKey=FlcmGMSuBSX9CQ32dU6EUtPBlbmctKJJ9kMsqvwdb1sxI/Y85Fz2y94CduA2ebJnbUWvI09P/QaE+AStR4g4jA==;EndpointSuffix=core.windows.net";
        private readonly string _fileShareName = "uploadfiles";
        private readonly string _directoryName = "uploads"; 

        public FileFunctions(ILogger<FileFunctions> logger)
        {
            _logger = logger;
        }

        // Function to list files
        [Function("ListFiles")]
        public async Task<IActionResult> ListFiles([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
        {
            var fileModels = new List<FileModel>();
            try
            {
                var serviceClient = new ShareServiceClient(_connectionString);
                var shareClient = serviceClient.GetShareClient(_fileShareName);
                var directoryClient = shareClient.GetDirectoryClient(_directoryName);

                await directoryClient.CreateIfNotExistsAsync(); 

                await foreach (ShareFileItem item in directoryClient.GetFilesAndDirectoriesAsync())
                {
                    if (!item.IsDirectory)
                    {
                        var fileClient = directoryClient.GetFileClient(item.Name);
                        var properties = await fileClient.GetPropertiesAsync();

                        fileModels.Add(new FileModel
                        {
                            Name = item.Name,
                            Size = properties.Value.ContentLength,
                            LastModified = properties.Value.LastModified
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error listing files: {ex.Message}");
                return new BadRequestObjectResult($"Error listing files: {ex.Message}");
            }

            return new OkObjectResult(fileModels);
        }

        // Function to upload a file
        [Function("UploadFile")]
        public async Task<IActionResult> UploadFile([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            var file = req.Form.Files[0];
            if (file == null || file.Length == 0)
            {
                return new BadRequestObjectResult("Please select a file to upload.");
            }

            try
            {
                using (var stream = file.OpenReadStream())
                {
                    var serviceClient = new ShareServiceClient(_connectionString);
                    var shareClient = serviceClient.GetShareClient(_fileShareName);
                    var directoryClient = shareClient.GetDirectoryClient(_directoryName);
                    await directoryClient.CreateIfNotExistsAsync(); 

                    var fileClient = directoryClient.GetFileClient(file.FileName);
                    await fileClient.CreateAsync(file.Length);
                    await fileClient.UploadRangeAsync(new HttpRange(0, file.Length), stream);
                }

                return new OkObjectResult($"File '{file.FileName}' uploaded successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError($"File upload failed: {ex.Message}");
                return new BadRequestObjectResult($"File upload failed: {ex.Message}");
            }
        }

        // Function to download a file
        [Function("DownloadFile")]
        public async Task<IActionResult> DownloadFile([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
        {
            string fileName = req.Query["fileName"];
            if (string.IsNullOrEmpty(fileName))
            {
                return new BadRequestObjectResult("File name cannot be null or empty.");
            }

            try
            {
                var serviceClient = new ShareServiceClient(_connectionString);
                var shareClient = serviceClient.GetShareClient(_fileShareName);
                var directoryClient = shareClient.GetDirectoryClient(_directoryName);
                var fileClient = directoryClient.GetFileClient(fileName);

                var downloadInfo = await fileClient.DownloadAsync();
                var memoryStream = new MemoryStream();
                await downloadInfo.Value.Content.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                return new FileStreamResult(memoryStream, "application/octet-stream")
                {
                    FileDownloadName = fileName
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error downloading file: {ex.Message}");
                return new BadRequestObjectResult($"Error downloading file: {ex.Message}");
            }
        }
    }
}

public class FileModel
{
    public string Name { get; set; }
    public long Size { get; set; }
    public DateTimeOffset? LastModified { get; set; }
}


//Mrzyg?ód, K., 2022. Azure for Developers. 2nd ed. August: [Meeta Rajani]