using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
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
    public static class AddSubscriber
    {
        private static bool IsValidEmail(this string email) => new EmailAddressAttribute().IsValid(email);

        [FunctionName("AddSubscriber")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "subscribe/")]
            HttpRequest req,
            [Table(TableConstants.Subscribers)] CloudTable subscribersTable,
            ILogger log)
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var subscriber = JsonConvert.DeserializeObject<Subscriber>(requestBody);

            if (subscriber == null)
                return new BadRequestResult();

            if (string.IsNullOrWhiteSpace(subscriber.Email) || !subscriber.Email.IsValidEmail())
                return new BadRequestResult();

            subscriber.RowKey = Guid.NewGuid().ToString();
            subscriber.PartitionKey = TableConstants.Subscribers;

            // TODO send verification email

            await subscribersTable.ExecuteAsync(TableOperation.Insert(subscriber));
            return new OkObjectResult(subscriber);
        }
    }
}