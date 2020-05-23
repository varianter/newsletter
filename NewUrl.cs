using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using newsletter.Model;
using Newtonsoft.Json;

namespace newsletter
{
    public static class NewUrl
    {
        private static bool IsValidUrl(this string url)
        {
            if (!Regex.IsMatch(url, @"^https?:\/\/", RegexOptions.IgnoreCase))
                url = "https://" + url;
            
            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }
        
        [FunctionName("NewUrl")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "url/")] HttpRequest req,
            [Table(TableConstants.NewsletterItems)] CloudTable newsletterItemsTable,
            ILogger log)
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var newsletterItem = JsonConvert.DeserializeObject<NewsletterItem>(requestBody);

            if(newsletterItem == null)
                return new BadRequestResult();
            
            if(string.IsNullOrWhiteSpace(newsletterItem.Url) || !newsletterItem.Url.IsValidUrl())
                return new BadRequestResult();

            newsletterItem.RowKey = Guid.NewGuid().ToString();
            newsletterItem.PartitionKey = TableConstants.NewsletterItems;
            
            await newsletterItemsTable.ExecuteAsync(TableOperation.Insert(newsletterItem));
            
            return new OkObjectResult(newsletterItem);
        }
    }
}
