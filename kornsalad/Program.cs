using Newtonsoft.Json;
using OpenCvSharp;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Util;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace kornsalad
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            //TestEffect();
            TestRPC();
            //TestEncode();

            Console.WriteLine("Completed.");
        }

        static void TestEncode()
        {
            List<Effector> effectors = new List<Effector>();

            for (int i = 0; i < 5; i++)
            {
                var e = new Effector(23.98, new Size(800, 600));
                e.Initialize("white");

                // test effects
                e.Earthquake(20, 2, 2);
                e.Shake(2, 1, 3);
                e.Rotate(90, false, 2);
                e.FullRotate(15, false, 2);
                e.Transition(400, 400, 5, 0, 0);

                effectors.Add(e);
            }

            var a = new Encoder(effectors.ToArray());
            a.SequentialMerge();
            a.Encode("mp4");
        }

        static void TestEffect()
        {
            var e = new Effector(30, new Size(1280, 800));
            e.Initialize("black");
            e.AddLayer("base.png").None(4)
             .AddLayer("alpharesized.png").Earthquake(16, 2, 5)
             .AddLayer("chat.png").Earthquake(10, 2, 2)
             .AddLayer("alpharesized.png", 10, 10).Earthquake(15, 2, 2);
            var file = e.Encode();
        }

        static void TestRPC()
        {
            RpcServer rpcserver = new RpcServer(
                host: "mq.actoon.sokdak.me",
                user: "actoon_enc_server",
                password: "zhekfl12!"
            );

            rpcserver.Initialize();
            rpcserver.ListenQueue("rpc_encoding_queue", (model , ea) =>
            {
                EventingBasicConsumer _model = (EventingBasicConsumer)model;
                var channel = _model.Model;

                var prop = channel.CreateBasicProperties();
                prop.CorrelationId = ea.BasicProperties.CorrelationId;

                var decodedB64 = Encoding.UTF8.GetString(Convert.FromBase64String(Encoding.UTF8.GetString(ea.Body)));
                var rpcRequest = JsonConvert.DeserializeObject<RpcRequest>(decodedB64);

                // business logics
                // TODO: write a logic that process encoding tasks
                // end of logics

                channel.BasicPublish(ea.Exchange, ea.RoutingKey, prop, null);
            });
        }
    }
}
