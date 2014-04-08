using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Producer
{
    class Program
    {
        const string requestQueueName = "RequestQueue";
        const string responseQueueName = "ReplyQueue";
        static void Main(string[] args)
        {
            // If you need to run this through more secured networks, you can uncomment the following line
            // ServiceBusEnvironment.SystemConnectivity.Mode = ConnectivityMode.Http;

            String.Format("Locator Application").Dump();


            // Verify queue existence
            CreateQueues();

            // Create the MessageReceiver and MessageSender
            var requestClient = QueueClient.Create(requestQueueName);

            var eventDrivenMessagingOptions = new OnMessageOptions();
            eventDrivenMessagingOptions.AutoComplete = true;
            eventDrivenMessagingOptions.ExceptionReceived += OnExceptionReceived;
            eventDrivenMessagingOptions.MaxConcurrentCalls = 5;
            requestClient.OnMessage(OnMessageArrived, eventDrivenMessagingOptions);


            String.Format("Listening for requests...").Dump();

            // Listening for requests
            BrokeredMessage requestMessage = requestClient.Receive(TimeSpan.FromSeconds(60));

            
        }

        private static void OnMessageArrived(BrokeredMessage requestMessage)
        {
            if (requestMessage != null)
            {
                var ipAddress = requestMessage.GetBody<string>();
                if (string.IsNullOrEmpty(ipAddress))
                {
                    "No IP Address provided".Dump();
                    return;
                }
                String.Format("Received request for IP address {0}...", ipAddress).Dump();

                // Do lookup on IP Address web service
                string location = "ok";

                BrokeredMessage responseMessage;

                // Create a response message with the actual location in the body (as string)
                responseMessage = new BrokeredMessage(location);
                String.Format("Sending response with location {0}", location).Dump();

                // Set the SessionId in the response message to the ReplyToSessionId
                // of the incoming message, and submit to the response queue.
                responseMessage.SessionId = requestMessage.ReplyToSessionId;
                var responseClient = QueueClient.Create(responseQueueName);
                responseClient.Send(responseMessage);

                // Complet the request as we have processed everything
                requestMessage.Complete();

                String.Format("Good bye!").Dump();
                Console.ReadLine();
            }
        }

        static void OnExceptionReceived(object sender, ExceptionReceivedEventArgs e)
        {
            e.ToString().Dump();
        }

        private static void CreateQueues()
        {
            // Create the NamespaceManager
            NamespaceManager namespaceMgr = NamespaceManager.Create();

            // If queue does not exist, create it
            if (!namespaceMgr.QueueExists(requestQueueName))
            {
                namespaceMgr.CreateQueue(requestQueueName);
            }
            if (!namespaceMgr.QueueExists(responseQueueName))
            {
                // The response queue requires sessions in order to filter out the correlated responses
                QueueDescription responseQueueDescription =
                    new QueueDescription(responseQueueName)
                    {
                        RequiresSession = true
                    };
                namespaceMgr.CreateQueue(responseQueueDescription);
            }
        }
    }

    internal static class StringExtensions
    {
        public static void Dump(this string value)
        {
            Console.WriteLine(value);
        }
    }
}
