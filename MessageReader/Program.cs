 using System;
 using System.Threading.Tasks;
 using Azure.Messaging.ServiceBus;
 namespace MessageReader
 {
     class Program
     {   
         static string connectionStringServiceBus = "<string de conexão do Service Bus na Azure >";
            
         static string filaNome = "<nome da fila criada no Service Bus na Azure>";
         static ServiceBusClient client= default;

         /* Inicializa um ServiceBusProcessor que será usado para processar as mensagens da fila */
         static ServiceBusProcessor processor = default;


         static async Task MessageHandler(ProcessMessageEventArgs args)
         {   
             /* Cria um static async "MessageHandler" task que cria o corpo das mensagens na fila
              ao passo em que são processadas e remove elas após cada processamento completo */

             string body = args.Message.Body.ToString();
             Console.WriteLine($"Recebido: {body}");
             await args.CompleteMessageAsync(args.Message);
         }
         static Task ErrorHandler(ProcessErrorEventArgs args)
         {   
             /* Cria um static async "ErrorHandler" task que gerencia qualquer
              exceção encontrada durante o processamento de mensagem */
             Console.WriteLine(args.Exception.ToString());
             return Task.CompletedTask;
         }

         static async Task Main()
         {   
             /* Inicializa "client" do tipo "ServiceBusClient" que vai prover conectividade
              no diretório do Service Bus e "processor" que será responsável por processar as mensagens */
             client = new ServiceBusClient(connectionStringServiceBus);
             processor = client.CreateProcessor(filaNome, new ServiceBusProcessorOptions());
             try
             {   
                 /* Bloco try que, primeiramente implementa um handler de mensagem e erro,
                 inicia o processamento da mensagem, e para o processamento seguindo o input do usuário */
                 processor.ProcessMessageAsync += MessageHandler;
                 processor.ProcessErrorAsync += ErrorHandler;

                 await processor.StartProcessingAsync();
                 Console.WriteLine("Ouvindo mensagens. Aperque qualquer tecla para parar o serviço.");
                 Console.ReadKey();

                 Console.WriteLine("\nParando serviço...");
                 await processor.StopProcessingAsync();
                 Console.WriteLine("Serviço parado.");
             }
             finally
             {
                /* Cria um bloco finally que, de maneira assíncrona, da um dispose nos objetos
                 "processor" e "client", aliviando qualquer recurso nao gerenciado de rede */
                 await processor.DisposeAsync();
                 await client.DisposeAsync();
             }
         }
     }
 }