using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

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

            String.Format("Locator 'processor' Application").Dump();


            // Verify queue existence
            CreateQueues();

            // Create the MessageReceiver and MessageSender
            var requestClient = QueueClient.Create(requestQueueName, ReceiveMode.PeekLock);

            var eventDrivenMessagingOptions = new OnMessageOptions();
            eventDrivenMessagingOptions.AutoComplete = false;
            eventDrivenMessagingOptions.ExceptionReceived += OnExceptionReceived;
            eventDrivenMessagingOptions.MaxConcurrentCalls = 5;
            requestClient.OnMessage(OnMessageArrived, eventDrivenMessagingOptions);


            String.Format("Listening for requests...").Dump();
            "Press enter to exit".Dump();
            Console.ReadLine();

        }

        private static void OnMessageArrived(BrokeredMessage requestMessage)
        {
            if (requestMessage != null)
            {
                var responseClient = QueueClient.Create(responseQueueName);

                var ipAddress = requestMessage.GetBody<string>();
                if (string.IsNullOrEmpty(ipAddress))
                {
                    "No IP Address provided".Dump();
                    requestMessage.DeadLetter();       
                    return;
                }

                String.Format("Received request for IP address {0}...", ipAddress).Dump();

                // Do lookup on IP Address web service
                string location = getCityForIp(ipAddress);
                
                var sessionId = requestMessage.ReplyToSessionId;

                // Create a response message with the actual location in the body (as string)
                BrokeredMessage responseMessage = new BrokeredMessage(location);
                String.Format("Sending response with location {0}", location).Dump();

                // Set the SessionId in the response message to the ReplyToSessionId
                // of the incoming message, and submit to the response queue.
                responseMessage.SessionId = sessionId;
                responseClient.Send(responseMessage);

                // Complete the request as we have processed everything
                requestMessage.Complete();

                String.Format("Good bye!").Dump();
                Console.ReadLine();
            }
        }

        private static string getCityForIp(string ipAddress)
        {
            string uri = string.Format("http://freegeoip.net/xml/{0}", ipAddress);
            var freeGeoIPRequest = HttpWebRequest.Create(uri);
            var response = freeGeoIPRequest.GetResponse();
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(response.GetResponseStream());
            var cityNode = xmlDoc.SelectSingleNode("/Response/City/text()");
            if (cityNode != null)
            {
                return cityNode.InnerText;
            }
            else
            {
                return "Nothing found";
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
