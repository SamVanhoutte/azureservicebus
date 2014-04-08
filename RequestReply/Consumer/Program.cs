using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Consumer
{
    class Program
    {
        const string requestQueueName = "RequestQueue";
        const string responseQueueName = "ReplyQueue";

        static void Main(string[] args)
        {
            // If you need to run this through more secured networks, you can uncomment the following line
            // ServiceBusEnvironment.SystemConnectivity.Mode = ConnectivityMode.Http;

            String.Format("Consumer Application").Dump();

            // Verify queue existence
            CreateQueues();

            // Create the MessageReceiver and MessageSender
            var requestClient = QueueClient.Create(requestQueueName);
            var responseClient = QueueClient.Create(responseQueueName);

            // Read ip address
            "Please enter an IP address you want to geo-locate".Dump();
            string ipAddress = Console.ReadLine();

            // Create correlation token for this specific request.
            string correlationId = Guid.NewGuid().ToString();

            // Pass the IP address as brokered message body (string serialization)
            // ReplyToSessionId will be used to listen on the reply queue with the correlated session
            BrokeredMessage requestMessage = new BrokeredMessage(ipAddress);
            requestMessage.ReplyToSessionId = correlationId;

            // Send the request message.
            String.Format("Sending request...").Dump();
            requestClient.Send(requestMessage);

            String.Format("Listening on session {0}...", correlationId).Dump();

            // Listen on the correlation session
            // This will make sure that only this specific receiver/thread will get the response message
            var session = responseClient.AcceptMessageSession(correlationId);

            String.Format("Receiving message...").Dump();

            // Handle response and close : timeout of 60 seconds
            var responseMessage = session.Receive(TimeSpan.FromSeconds(60));

            if (responseMessage != null)
            {
                var responseText = responseMessage.GetBody<string>();
                if (string.IsNullOrEmpty(responseText))
                {
                    String.Format("IP address {0} not located...", ipAddress).Dump();
                }
                else
                {
                    String.Format("IP address {0} resolved to {1}", ipAddress, responseText).Dump();
                }

                // Remove message from reply queue
                responseMessage.Complete();
            }
            // Close the MessageSession.
            session.Close();
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
