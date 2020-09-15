using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;
using AzureFunctions.Extensions.Swashbuckle.Attribute;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.WindowsAzure.Storage.Table;
using newsletter.Model;

namespace newsletter
{
    public static class AddSubscriber
    {
        private static bool IsValidEmail(this string email) => new EmailAddressAttribute().IsValid(email);
        
        [ProducesResponseType(typeof(ISubscriber), (int) HttpStatusCode.OK)]
        [FunctionName("AddSubscriber")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "subscribe/")]
            [RequestBodyType(typeof(ISubscriber), "Subscribe model")]
            Subscriber subscriber,
            [Table(TableConstants.Subscribers)] CloudTable subscribersTable)
        {
            if (subscriber == null)
                return new BadRequestResult();

            if (string.IsNullOrWhiteSpace(subscriber.Email) || !subscriber.Email.IsValidEmail())
                return new BadRequestResult();

            subscriber.RowKey = Guid.NewGuid().ToString();
            subscriber.PartitionKey = TableConstants.Subscribers;

            // TODO send verification email

            await subscribersTable.ExecuteAsync(TableOperation.Insert(subscriber));
            return new OkObjectResult(new { subscriber.Email });
        }
    }
}