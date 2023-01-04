using System.Text.Json;
using Azure.Messaging.ServiceBus;

namespace SBSender.Services
{
    public class QueueService : IQueueService
    {
        private readonly IConfiguration _configuration;

        public QueueService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendMessageAsync<T>(T serviceBusMessage, string queueName)
        {
            var client = new Azure.Messaging.ServiceBus.ServiceBusClient(
                _configuration.GetConnectionString("AzureServiceBus"),
                new ServiceBusClientOptions() { TransportType = ServiceBusTransportType.AmqpWebSockets });
            
            var sender = client.CreateSender(queueName);
            var messageBody =  JsonSerializer.SerializeToUtf8Bytes(serviceBusMessage);
            await sender.SendMessageAsync(new ServiceBusMessage(messageBody));
        }
    }
}