// See https://aka.ms/new-console-template for more information

using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SBShared;

var bulder = new ConfigurationBuilder();
BuildConfig(bulder);

var host = Host.CreateDefaultBuilder()
    .ConfigureServices((context, services) =>
    {
        // Register ur services
        services.AddTransient<IStartup, StartUp>();
    })
    .Build();

var ser = ActivatorUtilities.GetServiceOrCreateInstance<IStartup>(host.Services);

ser.Run();

static void BuildConfig(IConfigurationBuilder builder)
{
    builder.SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", false, true);
}

const string connectionString = "";
const string queueName = "personqueue";

var client = new ServiceBusClient(
    connectionString,
    new ServiceBusClientOptions() { TransportType = ServiceBusTransportType.AmqpWebSockets });

ServiceBusProcessor processor = client.CreateProcessor(queueName, new ServiceBusProcessorOptions());

processor.ProcessMessageAsync += MessageHandler;
processor.ProcessErrorAsync += ErrorHandler;

await processor.StartProcessingAsync();

Console.WriteLine("Wait for a minute and then press any key to end the processing");
Console.ReadKey();
await processor.StopProcessingAsync();

await processor.DisposeAsync();
await client.DisposeAsync();



async Task MessageHandler(ProcessMessageEventArgs args)
{
    Person body = args.Message.Body.ToObjectFromJson<Person>();
    Console.WriteLine($"Received: {body.FirstName} {body.LastName}");

    // complete the message. message is deleted from the queue. 
    await args.CompleteMessageAsync(args.Message);
}

// handle any errors when receiving messages
Task ErrorHandler(ProcessErrorEventArgs args)
{
    Console.WriteLine(args.Exception.ToString());
    return Task.CompletedTask;
}

public interface IStartup
{
    void Run();
}
public class StartUp:IStartup
{
    private readonly IConfiguration _configuration;

    public StartUp(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Run()
    {
        for (int i = 0; i < 100; i++)
        {
            Console.WriteLine(i);
        }

        Console.ReadKey();
    }
}