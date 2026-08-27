// Lists all stored operational files in the "staff-docs" blob container, returning file name, size and last modified date for all of them.
// Code attribution: blob listing pattern adapted from Microsoft's Azure Blob Storage documentation:
// Microsoft, "List blobs with .NET," Microsoft Learn, 2025.
// https://learn.microsoft.com/en-us/azure/storage/blobs/storage-blobs-list
// [Accessed: 27-Aug-2026].
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