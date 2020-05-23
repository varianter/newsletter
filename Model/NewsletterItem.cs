using Microsoft.WindowsAzure.Storage.Table;

namespace newsletter.Model
{
    public class NewsletterItem : TableEntity
    {
        public string Url { get; set; }
        public string Description { get; set; }
        public string Tags  { get; set; }
    }
}