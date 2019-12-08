using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace kornsalad
{
    class RpcServer
    {
        ConnectionFactory ConnectionInfo { get; set; }
        IConnection Connection { get; set; }
        IModel Channel { get; set; }

        public RpcServer(string host, string user, string password)
        {
            ConnectionInfo = new ConnectionFactory
            {
                HostName = host,
                Password = password,
                UserName = user,
                VirtualHost = "/"
            };
        }

        public void Initialize()
        {
            Connection = ConnectionInfo.CreateConnection();
            Channel = Connection.CreateModel();
        }

        public void Close()
        {
            Channel.Close();
            Channel.Dispose();

            Connection.Close();
            Connection.Dispose();
        }

        public void ListenQueue(string queueName, EventHandler<BasicDeliverEventArgs> onReceived)
        {
            Channel.QueueDeclare(
                queue: queueName,
                autoDelete: false
            );

            var consumer = new EventingBasicConsumer(Channel);
            consumer.Received += onReceived;

            Channel.BasicConsume(
                queue: queueName,
                autoAck: true,
                consumer: consumer
            );
        }
    }
}
