//using BarberShop.Application.Interfaces;
//using RabbitMQ.Client.Events;
//using RabbitMQ.Client;
//using System.Text;
//using BarberShop.Application.Services;
//using BarberShop.Domain.Entities;
//using Microsoft.Extensions.DependencyInjection;
//using Newtonsoft.Json; // Certifique-se de ter essa diretiva de using

//public class RabbitMQService : IRabbitMQService
//{
//    private readonly string _sendGridApiKey;
//    private readonly IServiceScopeFactory _serviceScopeFactory; // Alterado para IServiceScopeFactory
//    private readonly IConnection _connection;
//    private readonly IModel _channel;

//    public RabbitMQService(string sendGridApiKey, IServiceScopeFactory serviceScopeFactory) // Alterado para IServiceScopeFactory
//    {
//        _sendGridApiKey = sendGridApiKey;
//        _serviceScopeFactory = serviceScopeFactory;

//        var factory = new ConnectionFactory() { HostName = "localhost" };
//        _connection = factory.CreateConnection();
//        _channel = _connection.CreateModel();
//        _channel.QueueDeclare(queue: "emailQueue", durable: true, exclusive: false, autoDelete: false, arguments: null);
//    }

//    public void EnviarParaFila(string mensagem)
//    {
//        var body = Encoding.UTF8.GetBytes(mensagem);
//        _channel.BasicPublish(exchange: "", routingKey: "emailQueue", basicProperties: null, body: body);
//        Console.WriteLine("Mensagem enfileirada para envio de e-mail.");
//    }

//    public void IniciarConsumo()
//    {
//        var consumer = new EventingBasicConsumer(_channel);
//        consumer.Received += async (model, ea) =>
//        {
//            var body = ea.Body.ToArray();
//            var messageJson = Encoding.UTF8.GetString(body);

//            Console.WriteLine("Mensagem recebida da fila: " + messageJson);
//            if (string.IsNullOrWhiteSpace(messageJson))
//            {
//                Console.WriteLine("Mensagem vazia, ignorando.");
//                _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
//                return;
//            }

//            try
//            {
//                using (var scope = _serviceScopeFactory.CreateScope()) // Criar um escopo com IServiceScopeFactory
//                {
//                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>(); // Resolva o IEmailService dentro do escopo
//                    var emailMessage = JsonConvert.DeserializeObject<EmailMessage>(messageJson);

//                    if (emailMessage == null || string.IsNullOrEmpty(emailMessage.EmailCliente) || string.IsNullOrEmpty(emailMessage.EmailBarbeiro))
//                    {
//                        Console.WriteLine("Mensagem inválida, ignorando.");
//                        _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
//                        return;
//                    }

//                    // Envia e-mails para o cliente e barbeiro
//                    await emailService.EnviarEmailAgendamentoAsync(
//                        emailMessage.EmailCliente,
//                        emailMessage.NomeCliente,
//                        "Confirmação de Agendamento",
//                        emailMessage.ConteudoEmailCliente,
//                        emailMessage.NomeBarbeiro,
//                        emailMessage.DataHoraInicio,
//                        emailMessage.DataHoraFim,
//                        emailMessage.ValorTotal,
//                        emailMessage.GoogleCalendarLink);

//                    await emailService.EnviarEmailAgendamentoAsync(
//                        emailMessage.EmailBarbeiro,
//                        emailMessage.NomeBarbeiro,
//                        "Novo Agendamento",
//                        emailMessage.ConteudoEmailBarbeiro,
//                        emailMessage.NomeBarbeiro,
//                        emailMessage.DataHoraInicio,
//                        emailMessage.DataHoraFim,
//                        emailMessage.ValorTotal,
//                        emailMessage.GoogleCalendarLink);

//                    _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
//                }
//            }
//            catch (JsonReaderException ex)
//            {
//                Console.WriteLine("Erro ao desserializar a mensagem: " + ex.Message);
//                _channel.BasicNack(ea.DeliveryTag, false, false);
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine("Erro ao processar a mensagem: " + ex.Message);
//                _channel.BasicNack(ea.DeliveryTag, false, false);
//            }
//        };

//        _channel.BasicConsume(queue: "emailQueue", autoAck: false, consumer: consumer);
//        Console.WriteLine("Aguardando mensagens...");
//    }

//    public void Fechar()
//    {
//        _channel.Close();
//        _connection.Close();
//    }
//}
