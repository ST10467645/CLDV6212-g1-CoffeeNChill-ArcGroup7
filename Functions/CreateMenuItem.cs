// Code attribution: HTTP trigger + Table Storage insert pattern adapted from
// lecturer provided course example and Microsoft Learn:
// Microsoft, "HTTP trigger for Azure Functions," Microsoft Learn, 2025.
// https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-http-webhook-trigger
// [Accessed: 26-Aug-2026].
using Azure;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using System.Text.Json;
using CoffeeNChillFunctions.Models;

namespace CoffeeNChillFunctions.Functions
{
    public class CreateMenuItem
    {
        [Function("CreateMenuItem")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "menu")] HttpRequestData req)
        {
            var response = req.CreateResponse();
            try
            {
                string body = await new StreamReader(req.Body).ReadToEndAsync();
                var item = JsonSerializer.Deserialize<MenuItem>(body,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (item == null || string.IsNullOrWhiteSpace(item.PartitionKey) ||
                    string.IsNullOrWhiteSpace(item.RowKey) || string.IsNullOrWhiteSpace(item.Name))
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    await response.WriteStringAsync("Category, SKU (RowKey) and Name are required.");
                    return response;
                }

                var table = new TableClient("UseDevelopmentStorage=true", "MenuItems");
                await table.CreateIfNotExistsAsync();
                await table.AddEntityAsync(item);

                response.StatusCode = HttpStatusCode.Created;
                await response.WriteStringAsync("Menu item created successfully!");
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