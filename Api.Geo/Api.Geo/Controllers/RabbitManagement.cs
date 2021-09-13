using Api.Geo.Configuration;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Geo.Controllers
{
    public class RabbitManagement
    {
        private readonly RabbitMqConfiguration _rabbit;
        private readonly ConnectionFactory connectionFactory;
        public RabbitManagement(RabbitMqConfiguration rabbit)
        {
            _rabbit = rabbit;
            connectionFactory = new ConnectionFactory
            {
                UserName = _rabbit.UserName,
                Password = _rabbit.Password,
                HostName = _rabbit.Hostname,
                Port = Convert.ToInt32(_rabbit.Port)
            };
        }

        public void PublishMessage(object message)
        {
            using (var conn = connectionFactory.CreateConnection())
            {
                using (var channel = conn.CreateModel())
                {
                    channel.QueueDeclare(
                        queue: _rabbit.QueueName,
                        durable: false,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null
                    );

                    var jsonPayload = JsonConvert.SerializeObject(message);
                    var body = Encoding.UTF8.GetBytes(jsonPayload);

                    channel.BasicPublish(exchange: "",
                        routingKey: _rabbit.QueueName,
                        basicProperties: null,
                        body: body
                    );
                }
            }
        }
    }
}
