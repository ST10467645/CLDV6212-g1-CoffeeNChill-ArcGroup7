// Code attribution: directly adapted from lecturer-provided "Method for put
// and delete" course example, using the same PUT +
// ETag concurrency pattern, and Microsoft's official documentation:
// Microsoft, "Azure Tables client library for .NET," Microsoft Learn,6 May 2025.
// https://learn.microsoft.com/en-us/dotnet/api/overview/azure/data.tables-readme?view=azure-dotnet
// [Accessed: 28-Aug-2026].
using Azure;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using System.Text.Json;
using CoffeeNChillFunctions.Models;

namespace CoffeeNChillFunctions.Functions
{
    public class UpdateMenuItem
    {
        [Function("UpdateMenuItem")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "menu/{category}/{id}")] HttpRequestData req,
            string category, string id)
        {
            var response = req.CreateResponse();
            try
            {
                string body = await new StreamReader(req.Body).ReadToEndAsync();
                var updated = JsonSerializer.Deserialize<MenuItem>(body,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (updated == null)
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    await response.WriteStringAsync("Invalid JSON.");
                    return response;
                }

                var table = new TableClient("UseDevelopmentStorage=true", "MenuItems");

                Response<MenuItem> existing;
                try
                {
                    existing = await table.GetEntityAsync<MenuItem>(category, id);
                }
                catch (RequestFailedException ex) when (ex.Status == 404)
                {
                    response.StatusCode = HttpStatusCode.NotFound;
                    await response.WriteStringAsync("Menu item not found.");
                    return response;
                }

                updated.PartitionKey = category;
                updated.RowKey = id;
                updated.ETag = existing.Value.ETag;

                await table.UpdateEntityAsync(updated, updated.ETag, TableUpdateMode.Replace);

                response.StatusCode = HttpStatusCode.OK;
                await response.WriteStringAsync("Menu item updated successfully!");
                return response;
            }
            catch (RequestFailedException ex)
            {
                response.StatusCode = (HttpStatusCode)ex.Status;
                await response.WriteStringAsync($"Azure Table Error: {ex.ErrorCode} - {ex.Message}");
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