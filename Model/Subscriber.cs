using Microsoft.WindowsAzure.Storage.Table;

namespace newsletter.Model
{
    public class Subscriber : TableEntity
    {
        public string Email { get; set; }
    }
    

}