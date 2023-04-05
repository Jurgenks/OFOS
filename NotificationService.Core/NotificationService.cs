using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OFOS.Domain.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
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

                var userId = Guid.Empty;

                Notification notification = new(userId, emailMessage, "Email", "Received");

                _notificationRepository.CreateAsync(notification);
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur while processing the message
                // For example, log the error or send it to an error queue
            }
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
                    // For example, log the error or send it to an error queue

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
