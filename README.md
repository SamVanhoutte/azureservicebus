azureservicebus
===============

Request-Reply pattern.

In the request/reply pattern example, we have a producer sending a message to a request queue.  On the message, the ReplyToSessionId is specified and filled in with a correlation token.  The actual message consumer then receives the request and put the reply message on the response queue (that is Session-enabled).  On the response message, the SessionId is filled with the ReplyToSessionId from the request message.  The client application is then already listening on the response queue with the specific Session listener, using the correlation token.

Message deferal.

In the message deferal example, we have a producer that sends messages to a consumer, through an intermediary.  From producer to intermediary, service bus is used.  But the actual consumer for the data uses a plain REST interface to poll for a message.  Whenever his message is processed, he will then complete the message.  On the intermediary, the pull is done by reading one message from the queue and defering it.  Whenever the consumer wants to complete the message, the message will be deleted.
Things that will be added in a later version:
- TTL of messages
- Dead lettering of messages
