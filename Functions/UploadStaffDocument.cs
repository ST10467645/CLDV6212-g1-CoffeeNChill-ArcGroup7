// Code attribution: multipart-form parsing pattern adapted from Microsoft's
// MultipartReader documentation:
// Microsoft, "MultipartReader Class," Microsoft Learn, 2025.
// https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.webutilities.multipartreader
// [Accessed: 26-Aug-2026].
// Blob upload pattern adapted from lecturer provided blob storage example and Microsoft Azure Blob Storage documentation:
// Microsoft, "Introduction to Azure Blob Storage," Microsoft Learn, 2025.
// https://learn.microsoft.com/en-us/azure/storage/blobs/storage-blobs-introduction
// [Accessed: 26-Aug-2026].
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Net.Http.Headers;
using System.Net;

namespace CoffeeNChillFunctions.Functions
{
    public class UploadStaffDocument
    {
        [Function("UploadStaffDocument")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "documents/upload")] HttpRequestData req)
        {
            var response = req.CreateResponse();
            try
            {
                if (!req.Headers.TryGetValues("Content-Type", out var contentTypeValues))
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    await response.WriteStringAsync("Missing Content-Type header.");
                    return response;
                }

                var contentType = MediaTypeHeaderValue.Parse(contentTypeValues.First());
                var boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary).Value;

                if (string.IsNullOrWhiteSpace(boundary))
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    await response.WriteStringAsync("Request must be multipart/form-data with a valid boundary.");
                    return response;
                }

                var reader = new MultipartReader(boundary, req.Body);
                MultipartSection? section;
                string? uploadedFileName = null;

                while ((section = await reader.ReadNextSectionAsync()) != null)
                {
                    var contentDisposition = ContentDispositionHeaderValue.Parse(section.ContentDisposition);

                    if (contentDisposition.DispositionType.Equals("form-data") &&
                        !string.IsNullOrEmpty(contentDisposition.FileName.Value))
                    {
                        uploadedFileName = Path.GetFileName(contentDisposition.FileName.Value);

                        var allowed = new[] { "application/pdf", "image/png", "image/jpeg" };
                        if (!string.IsNullOrEmpty(section.ContentType) && !allowed.Contains(section.ContentType))
                        {
                            response.StatusCode = HttpStatusCode.BadRequest;
                            await response.WriteStringAsync($"File type '{section.ContentType}' not allowed.");
                            return response;
                        }

                        var containerClient = new BlobContainerClient("UseDevelopmentStorage=true", "staff-docs");
                        await containerClient.CreateIfNotExistsAsync();

                        var blobClient = containerClient.GetBlobClient(uploadedFileName);
                        await blobClient.UploadAsync(section.Body, overwrite: true);
                    }
                }

                if (uploadedFileName == null)
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    await response.WriteStringAsync("No file found in the request.");
                    return response;
                }

                response.StatusCode = HttpStatusCode.Created;
                await response.WriteStringAsync($"'{uploadedFileName}' uploaded successfully!");
                return response;
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                await response.WriteStringAsync($"Error: {ex.Message}");
                return response;
            }
        }
    }
}
