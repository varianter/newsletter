using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using newsletter.Model;
using SendGrid.Helpers.Mail;

namespace newsletter
{
    public static class SendNewsletter
    {
        private const string TheFirstOfEveryMonth = "0 0 0 1 * *";

        // For debugging
        private const string EveryThirtySeconds = "*/30 * * * * *";

        [FunctionName("SendNewsletter")]
        public static async void Run([TimerTrigger(TheFirstOfEveryMonth)] TimerInfo myTimer, ILogger log,
            [SendGrid()] IAsyncCollector<SendGridMessage> sendGrid,
            [Table(TableConstants.Subscribers)] CloudTable subscribersTable,
            [Table(TableConstants.NewsletterItems)]
            CloudTable newsletterItemsTable)
        {
            var newsletterItems = new List<NewsletterItem>();
            TableContinuationToken newsletterItemToken = null;

            do
            {
                var queryResult =
                    await newsletterItemsTable.ExecuteQuerySegmentedAsync(new TableQuery<NewsletterItem>(),
                        newsletterItemToken);
                newsletterItems.AddRange(queryResult.Results);
                newsletterItemToken = queryResult.ContinuationToken;
            } while (newsletterItemToken != null);

            var emailBody = string.Join(", ", newsletterItems.Select(item => item.Url));

            var subscribers = new List<Subscriber>();
            TableContinuationToken token = null;

            do
            {
                var queryResult =
                    await subscribersTable.ExecuteQuerySegmentedAsync(new TableQuery<Subscriber>(), token);
                subscribers.AddRange(queryResult.Results);
                token = queryResult.ContinuationToken;
            } while (token != null);

            var fromEmailAddress = Environment.GetEnvironmentVariable("SENDGRID_FROM_EMAIL_ADDRESS");
            foreach (var subscriber in subscribers)
            {
                var emailMessage = new SendGridMessage();
                emailMessage.AddTo(subscriber.Email);
                emailMessage.AddContent("text/html", emailBody);
                emailMessage.SetFrom(new EmailAddress(fromEmailAddress));
                emailMessage.SetSubject("Newsletter");

                await sendGrid.AddAsync(emailMessage);
            }

            await sendGrid.FlushAsync();
        }
    }
}