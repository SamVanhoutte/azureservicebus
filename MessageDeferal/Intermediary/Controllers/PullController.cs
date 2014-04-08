using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Intermediary.Controllers
{
    public class PullController : Controller
    {
        [HttpDelete]
        public async Task<HttpResponseMessage> Complete(int consumer, int messageid)
        {
            try
            {
                // Listen on the queue, specific for the consumer
                var queueClient = QueueClient.Create(string.Format("consumer-{0000}", consumer), ReceiveMode.ReceiveAndDelete);
                // Complete message
                await queueClient.ReceiveAsync(messageid);
                return new HttpResponseMessage(HttpStatusCode.Accepted);
            }
            catch (Exception ex)
            {
                // Log error here
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }

        [HttpPut]
        public async Task<HttpResponseMessage> Receive(int consumer)
        {
            try
            {
                // Listen on the queue, specific for the consumer
                var queueClient = QueueClient.Create(string.Format("consumer-{0000}", consumer), ReceiveMode.PeekLock);
                // Receive message, without waiting
                var message = await queueClient.ReceiveAsync(TimeSpan.FromMilliseconds(30));
                if (message == null)
                {
                    // Return no content found, when no message is found
                    return new HttpResponseMessage(HttpStatusCode.NoContent);
                }
                else
                {
                    // Create response with the data
                    var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                    // This message id will be used to complete the message
                    response.Headers.Add("MessageId", message.MessageId);
                    // Stream content, can be anything 
                    response.Content = new StreamContent(message.GetBody<MemoryStream>());
                    // We defer the message and modify a property to indicate that it was returned already
                    await message.DeferAsync(
                        new Dictionary<string, object> 
                    { 
                        { "Peeked", "true" } 
                    });
                    // Return message
                    return response;
                }
            }
            catch (Exception ex)
            {
                // Log error here
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }
    }
}
