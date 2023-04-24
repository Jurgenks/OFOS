using Newtonsoft.Json;
using OFOS.Domain.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace NotificationService.Core
{
    public class NotificationService : INotificationService
    {
        private readonly IConnection _rabbitConnection;
        private readonly IModel _rabbitChannel;
        private readonly INotificationRepository _notificationRepository;

        public NotificationService(INotificationRepository notificationRepository, IConnection rabbitConnection)
        {
            _rabbitConnection = rabbitConnection;
            _rabbitChannel = _rabbitConnection.CreateModel();
            _notificationRepository = notificationRepository;

        }

        public async Task StartAsync()
        {
            //Register Queue
            _rabbitChannel.QueueDeclare(queue: "email-queue",
                         durable: false,
                         exclusive: false,
                         autoDelete: false,
                         arguments: null);

            // Register the message consumer
            var consumer = new EventingBasicConsumer(_rabbitChannel);
            consumer.Received += HandleIncomingMessage;
            _rabbitChannel.BasicConsume(queue: "email-queue", autoAck: false, consumer: consumer);

            // Wait for the connection to be established
            while (!_rabbitConnection.IsOpen)
            {
                await Task.Delay(1000);
            }
        }


        private void HandleIncomingMessage(object sender, BasicDeliverEventArgs e)
        {
            // Get the message body and deserialize it
            var body = e.Body.ToArray();
            var message = JsonConvert.DeserializeObject<EmailMessage>(Encoding.UTF8.GetString(body));

            // Process the message
            var notification = new Notification(message.Body, "Email", "Received");
            _notificationRepository.CreateAsync(notification);

            try
            {
                SendEmailMessage(message);
                _rabbitChannel.BasicAck(e.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _rabbitChannel.BasicReject(e.DeliveryTag, true);
            }
        }

        public void ConsumeEmailMessage(string emailMessage)
        {
            try
            {
                var email = JsonConvert.DeserializeObject<EmailMessage>(emailMessage);

                Notification notification = new(emailMessage, "Email", "Received");

                SendEmailMessage(email);

                _notificationRepository.CreateAsync(notification);
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur while processing the message
                Console.WriteLine(ex.ToString());
            }
        }

        public void SendEmailMessage(EmailMessage mail)
        {
            var client = new SmtpClient("sandbox.smtp.mailtrap.io", 2525)
            {
                Credentials = new NetworkCredential("7f2e6b3b42281b", "021e06df2a4f7b"),
                EnableSsl = true
            };

            var message = new MailMessage("test@ofos.nl", mail.To, mail.Subject, mail.Body);
            client.Send(message);

        }


        public void Dispose()
        {
            _rabbitChannel.Dispose();
            _rabbitConnection.Dispose();
        }
    }

}
