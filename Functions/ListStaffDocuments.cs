// Lists all stored operational files in the "staff-docs" blob container, returning file name, size, and last modified date for each.
// Code attribution: blob listing pattern adapted from lecturer-provided blob storage example and Microsoft's official
// Azure Blob Storage documentation:
// Microsoft, "Introduction to Azure Blob Storage," Microsoft Learn, 2025.
// https://learn.microsoft.com/en-us/azure/storage/blobs/storage-blobs-introduction
// [Accessed: 28-Aug-2026].
using Azure;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace CoffeeNChillFunctions.Functions
{
    public class ListStaffDocuments
    {
        private readonly ILogger _logger;

        public ListStaffDocuments(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ListStaffDocuments>();
        }

        [Function("ListStaffDocuments")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "documents")] HttpRequestData req)
        {
            _logger.LogInformation("Listing staff documents started.");

            var response = req.CreateResponse();
            try
            {
                var containerClient = new BlobContainerClient("UseDevelopmentStorage=true", "staff-docs");
                await containerClient.CreateIfNotExistsAsync();

                var files = new List<object>();
                await foreach (var blobItem in containerClient.GetBlobsAsync())
                {
                    files.Add(new
                    {
                        FileName = blobItem.Name,
                        SizeInBytes = blobItem.Properties.ContentLength,
                        LastModified = blobItem.Properties.LastModified
                    });
                }

                _logger.LogInformation($"Found {files.Count} staff document(s).");
                response.StatusCode = HttpStatusCode.OK;
                await response.WriteAsJsonAsync(files);
                return response;
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError($"Azure Blob Storage error listing documents: {ex.ErrorCode} - {ex.Message}");
                response.StatusCode = (HttpStatusCode)ex.Status;
                await response.WriteStringAsync($"Azure Blob Storage Error: {ex.ErrorCode} - {ex.Message}");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error listing staff documents: {ex.Message}");
                response.StatusCode = HttpStatusCode.InternalServerError;
                await response.WriteStringAsync($"Error: {ex.Message}");
                return response;
            }
        }
    }
}