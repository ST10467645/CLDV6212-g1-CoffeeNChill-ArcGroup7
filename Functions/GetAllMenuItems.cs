// Code attribution: Table Storage query pattern adapted from lecturer-provided
// course example and Microsoft's official documentation:
// Microsoft, "Azure Tables client library for .NET," Microsoft Learn, 2025.
// https://learn.microsoft.com/en-us/dotnet/api/overview/azure/data.tables-readme?view=azure-dotnet
// [Accessed: 27-Aug-2026].
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using CoffeeNChillFunctions.Models;

namespace CoffeeNChillFunctions.Functions
{
    public class GetAllMenuItems
    {
        [Function("GetAllMenuItems")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "menu")] HttpRequestData req)
        {
            var response = req.CreateResponse(HttpStatusCode.OK);
            var table = new TableClient("UseDevelopmentStorage=true", "MenuItems");
            await table.CreateIfNotExistsAsync();

            var items = new List<MenuItem>();
            await foreach (var item in table.QueryAsync<MenuItem>())
                items.Add(item);

            await response.WriteAsJsonAsync(items);
            return response;
        }
    }
}