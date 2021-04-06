using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using System;
using System.Text;

string producerKey = "";
var producer = new EventHubProducerClient(new EventHubConnection(producerKey, new EventHubConnectionOptions
{
    TransportType = EventHubsTransportType.AmqpTcp
}));

var batch = await producer.CreateBatchAsync();

batch.TryAdd(new EventData(Encoding.UTF8.GetBytes("Hello, Event Hubs!")));
batch.TryAdd(new EventData(Encoding.UTF8.GetBytes("The middle event is this one")));
batch.TryAdd(new EventData(Encoding.UTF8.GetBytes("Goodbye, Event Hubs!")));

await producer.SendAsync(batch);

Console.ReadLine();