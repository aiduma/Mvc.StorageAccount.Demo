using Azure.Storage.Queues;
using Mvc.StorageAccount.Demo.Models;
using Newtonsoft.Json;

namespace Mvc.StorageAccount.Demo.Services
{
    public class QueuService : IQueueService
    {
        private readonly IConfiguration _configuration;
        private readonly QueueClient _queueClient;
        //private string queueName = "attendee-emails";

        public QueuService(IConfiguration configuration, QueueClient queueClient)
        {
            _configuration = configuration;
            _queueClient = queueClient;
        }

        public async Task SendMessage(EmailMessage email)
        {
            //var queueClient = new QueueClient(
            //    _configuration["StorageConnectionString"],
            //    queueName,
            //    new QueueClientOptions
            //    {
            //        MessageEncoding = QueueMessageEncoding.Base64
            //    }
            //);

            await _queueClient.CreateIfNotExistsAsync();
            var message = JsonConvert.SerializeObject(email);
            await _queueClient.SendMessageAsync(message);
        }
    }
}
