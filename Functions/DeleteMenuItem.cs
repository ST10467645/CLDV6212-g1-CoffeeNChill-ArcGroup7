// Code attribution: HTTP DELETE + Table Storage delete pattern adapted from
// lecturer-provided course example and Microsoft's
// official documentation:
// Microsoft, "Azure Tables client library for .NET," Microsoft Learn, 6 May 2025. [Online]
// https://learn.microsoft.com/en-us/dotnet/api/overview/azure/data.tables-readme?view=azure-dotnet
// [Accessed: 28-Aug-2026].
using Azure;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

namespace CoffeeNChillFunctions.Functions
{
    public class DeleteMenuItem
    {
        [Function("DeleteMenuItem")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "menu/{category}/{id}")] HttpRequestData req,
            string category, string id)
        {
            var response = req.CreateResponse();
            try
            {
                var table = new TableClient("UseDevelopmentStorage=true", "MenuItems");

                // --- check it exists first, so we can return a proper 404 (top rubric band) ---
                try
                {
                    await table.GetEntityAsync<Azure.Data.Tables.TableEntity>(category, id);
                }
                catch (RequestFailedException ex) when (ex.Status == 404)
                {
                    response.StatusCode = HttpStatusCode.NotFound;
                    await response.WriteStringAsync($"Menu item '{id}' in category '{category}' not found.");
                    return response;
                }

                await table.DeleteEntityAsync(category, id);

                response.StatusCode = HttpStatusCode.OK;
                await response.WriteStringAsync("Menu item deleted successfully!");
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