 using System;
 using System.Threading.Tasks;
 using Azure.Messaging.ServiceBus;
 namespace MessagePublisher
 {
     public class Program
     {
         private const string connectionStringServiceBus = "<string connection do recurso na Azure>";

         private const string nomeFila = "<nome da sua fila criada no recurso da Azure>";

         private const int numMensagens = 100;

         /* Inicializa um Service Bus client que será dono da conexão da fila alvo */
         static ServiceBusClient client = default!;

         /* Inicializa um Service Bus sender que será usado para enviar(publicar) as mensagens na fila alvo */
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

             /* Adiciona uma mensagem no batch e da um throw em uma exceção caso ultrapasse o limite do batch suportado */
             for (int i = 1; i <= numMensagens; i++)
             {
                 if (!messagemBatch.TryAddMessage(new ServiceBusMessage($"Message {i}")))
                 {
                     throw new Exception($"The message {i} is too large to fit in the batch.");
                 }
             }
             try
             {
                /* Try com o "sender" enviando as mensagens de maneira assíncrona no batch enfileirando no alvo */
                 await sender.SendMessagesAsync(messagemBatch);
                 Console.WriteLine($"A batch of {numMensagens} messages has been published to the queue.");
             }
             finally
             {
                 /* Finally que da um dispose assíncrono no "sender e no "client", aliviando 
                 qualquer recurso nao gerenciado de rede */
                 await sender.DisposeAsync();
                 await client.DisposeAsync();
             }
         }
     }
 }