using Azure.Messaging.EventHubs.Consumer;
using consumer;
using System;
using System.Text;
using System.Threading;

var storage = "";
var containerName = "azehcheckpoints";

var consumerKey = "";
var hubName = "snovahub";

//await EventListeners.ListenAsync(consumerKey, hubName);

await EventListeners.ProcessAsync(storage, containerName, consumerKey, hubName);