using Azure.Messaging.EventHubs.Producer;
using Azure.Messaging.EventHubs;
using Microsoft.Extensions.Configuration;
using System.Text;

int numOfEvents = 100;

var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("settings.json", optional: false, reloadOnChange: true)
                .Build();

var EVENT_HUB_CONNECTION_STRING = configuration["AppSettings:EVENT_HUB_CONNECTION_STRING"];
var EVENT_HUB_INSTANCE_NAME = configuration["AppSettings:EVENT_HUB_INSTANCE_NAME"];
var EVENT_HUB_PARTITION_KEY = configuration["AppSettings:EVENT_HUB_PARTITION_KEY"];

Console.WriteLine($"EVENT_HUB_INSTANCE_NAME: {EVENT_HUB_INSTANCE_NAME}");
Console.WriteLine($"EVENT_HUB_PARTITION_KEY: {EVENT_HUB_PARTITION_KEY}");

await using (var producerClient = new EventHubProducerClient(EVENT_HUB_CONNECTION_STRING, EVENT_HUB_INSTANCE_NAME))
{
    using EventDataBatch eventBatch = await producerClient.CreateBatchAsync(new CreateBatchOptions { PartitionKey = EVENT_HUB_PARTITION_KEY });

    for (int i = 0; i < numOfEvents; i++)
    {
        if (!eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes($"My sample event {i}"))))
        {
            // if it is too large for the batch
            throw new Exception($"Event {i} is too large for the batch and cannot be sent.");
        }
    }

    // Use the producer client to send the batch of events to the event hub
    await producerClient.SendAsync(eventBatch);
    Console.WriteLine($"A batch of {numOfEvents} events has been published.");
}