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
        const string queueName = "consumerQueue";
        static void Main(string[] args)
        {
            // Make sure the queue exists.
            var nsManager = NamespaceManager.Create();
            if (!nsManager.QueueExists(queueName))
                nsManager.CreateQueue(queueName);

            // Read number of messages to send
            "Please enter the number of messages you want to send".Dump();
            var enteredValue = Console.ReadLine();
            int messageCount = 0;
            if (int.TryParse(enteredValue, out messageCount))
            {
                // Loop for 1 to entered value
                var queueClient = QueueClient.Create(queueName);
                for (int id = 1; id <= messageCount; id++)
                {
                    string.Format("Sending message with id {0}", id).Dump();

                    // Create a new message and add a message property to id
                    var message = new BrokeredMessage { TimeToLive = TimeSpan.FromSeconds(120) };
                    message.Properties.Add("CustomerId", id);
                    // Send the message
                    queueClient.Send(message);
                }
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
