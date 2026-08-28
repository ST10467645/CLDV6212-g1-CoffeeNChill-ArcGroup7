// Shows a requested staff document (recipe sheet, cleaning manual, safety policy) back to the user from the "staff-docs" blob container.
// Code attribution: blob streaming download pattern adapted from Microsoft's Azure Blob Storage documentation:
// Microsoft, "Download a blob with .NET," Microsoft Learn, 2025.
// https://learn.microsoft.com/en-us/azure/storage/blobs/storage-blob-download
// [Accessed: 28-Aug-2026].
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace CoffeeNChillFunctions.Functions
{
    public class DownloadStaffDocument
    {
        private readonly ILogger _logger;

        public DownloadStaffDocument(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<DownloadStaffDocument>();
        }

        [Function("DownloadStaffDocument")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "documents/download/{fileName}")] HttpRequestData req,
            string fileName)
        {
            _logger.LogInformation($"Download requested for '{fileName}'.");

            var response = req.CreateResponse();

            try
            {
                var containerClient = new BlobContainerClient("UseDevelopmentStorage=true", "staff-docs");
                var blobClient = containerClient.GetBlobClient(fileName);

                if (!await blobClient.ExistsAsync())
                {
                    _logger.LogWarning($"File '{fileName}' not found.");
                    response.StatusCode = HttpStatusCode.NotFound;
                    await response.WriteStringAsync($"File '{fileName}' not found.");
                    return response;
                }

                var downloadResult = await blobClient.DownloadStreamingAsync();

                response.StatusCode = HttpStatusCode.OK;
                response.Headers.Add("Content-Type", downloadResult.Value.Details.ContentType ?? "application/octet-stream");
                await downloadResult.Value.Content.CopyToAsync(response.Body); // streamed, not fully buffered

                _logger.LogInformation($"File '{fileName}' downloaded successfully.");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error downloading '{fileName}': {ex.Message}");
                response.StatusCode = HttpStatusCode.InternalServerError;
                await response.WriteStringAsync($"Error: {ex.Message}");
                return response;
            }
        }
    }
}