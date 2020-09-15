using Microsoft.WindowsAzure.Storage.Table;

namespace newsletter.Model
{
    public interface ISubscriber
    {
        public string Email { get; set; }
    }
    
    public class Subscriber : TableEntity, ISubscriber
    {
        public string Email { get; set; }
    }
}