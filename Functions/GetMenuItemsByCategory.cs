// Code attribution: filtered Table Storage query pattern adapted from
// lecturer-provided course example (CLDV6212, IIE, 2026) and Microsoft's
// official documentation:
// Microsoft, "Understand the Azure Table storage data model," Microsoft Learn, 2025. 
// https://learn.microsoft.com/en-us/azure/storage/tables/table-storage-overview
// [Accessed: 27-Aug-2026].
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using CoffeeNChillFunctions.Models;

namespace CoffeeNChillFunctions.Functions
{
    public class GetMenuItemsByCategory
    {
        [Function("GetMenuItemsByCategory")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "menu/category/{category}")] HttpRequestData req,
            string category)
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                var badReq = req.CreateResponse(HttpStatusCode.BadRequest);
                await badReq.WriteStringAsync("Category must be provided in the route.");
                return badReq;
            }

            var table = new TableClient("UseDevelopmentStorage=true", "MenuItems");
            await table.CreateIfNotExistsAsync();

            var items = new List<MenuItem>();
            await foreach (var item in table.QueryAsync<MenuItem>(x => x.PartitionKey == category))
                items.Add(item);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(items); // empty array [] if none found — valid, not an error
            return response;
        }
    }
}
