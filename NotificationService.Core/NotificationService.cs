using Newtonsoft.Json;
using OFOS.Domain.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Net.Mail;
using System.Net;
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

            RegisterMessageConsumers();
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

        private static void SendEmailMessage(EmailMessage mail)
        {
            using var client = new SmtpClient("smtp.mail.com", 587);
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential("your-email@mail.com", "your-password");

            var message = new MailMessage("your-email@mail.com", mail.To, mail.Subject, mail.Body);
            client.Send(message);

        }

        private void RegisterMessageConsumers()
        {
            var consumer = new EventingBasicConsumer(_rabbitChannel);
            consumer.Received += (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);

                    ConsumeEmailMessage(message);

                    // Acknowledge the message to remove it from the queue
                    _rabbitChannel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    // Handle any exceptions that occur while processing the message
                    Console.WriteLine(ex.ToString());

                    // Reject the message to put it back into the queue (optional)
                    _rabbitChannel.BasicReject(ea.DeliveryTag, true);
                }
            };

            _rabbitChannel.BasicConsume(queue: "email-queue", autoAck: false, consumer: consumer);
        }

        public void Dispose()
        {
            _rabbitChannel.Dispose();
            _rabbitConnection.Dispose();
        }
    }

}
