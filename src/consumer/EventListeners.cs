using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Processor;
using Azure.Storage.Blobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace consumer
{
    public class EventListeners
    {
        public static async Task ListenAsync(string consumerKey, string hubName)
        {
            var consumerClient = new EventHubConsumerClient(EventHubConsumerClient.DefaultConsumerGroupName, consumerKey, hubName);
            using var cancellationSource = new CancellationTokenSource();
            cancellationSource.CancelAfter(TimeSpan.FromSeconds(90));

            int eventsRead = 0;
            int maximumEvents = 3;

            await foreach (PartitionEvent partitionEvent in consumerClient.ReadEventsAsync(cancellationSource.Token))
            {
                Console.WriteLine($"Event Read: { Encoding.UTF8.GetString(partitionEvent.Data.Body.ToArray()) }");
                eventsRead++;

                if (eventsRead >= maximumEvents)
                {
                    break;
                }
            }
        }


        public static async Task ProcessAsync(string blobStorageConnectionString, string blobContainerName,
            string ehubNamespaceConnectionString, string eventHubName)
        {
            // Read from the default consumer group: $Default
            var consumerGroup = EventHubConsumerClient.DefaultConsumerGroupName;
            // Create a blob container client that the event processor will use 
            var storageClient = new BlobContainerClient(blobStorageConnectionString, blobContainerName);
            // Create an event processor client to process events in the event hub
            var processor = new EventProcessorClient(storageClient, consumerGroup, ehubNamespaceConnectionString, eventHubName);

            // Register handlers for processing events and handling errors
            processor.ProcessEventAsync += ProcessEventAsync;
            processor.ProcessErrorAsync += ProcessErrorAsync;

            // Start the processing
            await processor.StartProcessingAsync();
            // Wait for 10 seconds for the events to be processed
            await Task.Delay(TimeSpan.FromSeconds(10));
            // Stop the processing
            await processor.StopProcessingAsync();
        }

        private static Task ProcessErrorAsync(ProcessErrorEventArgs eventArgs)
        {
            // Write details about the error to the console window
            Console.WriteLine($"\tPartition '{ eventArgs.PartitionId}': an unhandled exception was encountered. This was not expected to happen.");
            Console.WriteLine(eventArgs.Exception.Message);
            return Task.CompletedTask;
        }

        private async static Task ProcessEventAsync(ProcessEventArgs eventArgs)
        {
            // Write the body of the event to the console window
            Console.WriteLine("\tReceived event: {0}", Encoding.UTF8.GetString(eventArgs.Data.Body.ToArray()));
            // Update checkpoint in the blob storage so that the app receives only new events the next time it's run
            await eventArgs.UpdateCheckpointAsync(eventArgs.CancellationToken);
        }
    }
}
