using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AzureFunctions.Extensions.Swashbuckle.Attribute;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.WindowsAzure.Storage.Table;
using newsletter.Model;

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
        
        [ProducesResponseType(typeof(INewsletterItem), (int) HttpStatusCode.OK)]
        [FunctionName("NewUrl")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "url/")] 
            [RequestBodyType(typeof(INewsletterItem), "Newsletter model")]
            NewsletterItem newsletterItem,
            [Table(TableConstants.NewsletterItems)] CloudTable newsletterItemsTable)
        {
            if(newsletterItem == null)
                return new BadRequestResult();
            
            if(string.IsNullOrWhiteSpace(newsletterItem.Url) || !newsletterItem.Url.IsValidUrl())
                return new BadRequestResult();

            newsletterItem.RowKey = Guid.NewGuid().ToString();
            newsletterItem.PartitionKey = TableConstants.NewsletterItems;
            
            await newsletterItemsTable.ExecuteAsync(TableOperation.Insert(newsletterItem));
            
            return new OkObjectResult(new
            {
                newsletterItem.Url,
                newsletterItem.Description,
                newsletterItem.Tags
            });
        }
    }
}
