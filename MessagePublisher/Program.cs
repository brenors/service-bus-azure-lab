 using System;
 using System.Threading.Tasks;
 using Azure.Messaging.ServiceBus;
 namespace MessagePublisher
 {
     public class Program
     {
         private const string connectionStringServiceBus = "<string connection with azure service bus>";

         private const string nomeFila = "messagequeue";

         private const int numMensagens = 100;

         /* To create a Service Bus client that will own the connection to the target queue */
         static ServiceBusClient client = default!;

         /* To create a Service Bus sender that will be 
            used to publish messages to the target queue */
         static ServiceBusSender sender = default!;

         public static async Task Main(string[] args)
         {   
             /* Inicializa um client do tipo "ServiceBusClient" que irá 
             prover conectividade ao diretório do Service Bus e ao "sender" que será responsável
             por enviar as mensagens */
             client = new ServiceBusClient(connectionStringServiceBus);
             sender = client.CreateSender(nomeFila);

             /* Inicialia um objeto do tipo "ServiceBusMessageBatch" que irá permitir a combinação múltiplas
              de mensagens dentro do batch usando o método "TryAddMessage" */
             using ServiceBusMessageBatch messagemBatch = await sender.CreateMessageBatchAsync();

             /* To add messages to a batch and throw an exception if a message
                size exceeds the limits supported by the batch */
             for (int i = 1; i <= numMensagens; i++)
             {
                 if (!messagemBatch.TryAddMessage(new ServiceBusMessage($"Message {i}")))
                 {
                     throw new Exception($"The message {i} is too large to fit in the batch.");
                 }
             }
             try
             {
                 /* To create a try block, with "sender" asynchronously 
                    publishing messages in the batch to the target queue */
                 await sender.SendMessagesAsync(messagemBatch);
                 Console.WriteLine($"A batch of {numMensagens} messages has been published to the queue.");
             }
             finally
             {
                 /* To create a finally block that asynchronously disposes of the "sender"
                    and "client" objects, releasing any network and unmanaged resources */
                 await sender.DisposeAsync();
                 await client.DisposeAsync();
             }
         }
     }
 }