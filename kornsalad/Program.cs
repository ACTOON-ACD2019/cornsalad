using RabbitMQ.Client;
using System;
using System.Collections.Generic;

namespace kornsalad
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            TestEffect();
            //TestRPC();
            //TestEncode();

            Console.WriteLine("Completed.");
        }
        /*
        static void TestEncode()
        {
            List<Effector> effectors = new List<Effector>();

            for (int i = 0; i < 5; i++)
            {
                var e = new Effector("demo" + i + ".jpg", 24);
                e.Initialize();

                // test effects
                e.Earthquake(20, 2, 2);
                e.Shake(2, 1, 3);
                e.Rotate(90, false, 2);
                e.FullRotate(15, false, 2);
                e.Transition(400, 400, 5, 0, 0);

                e.Close();
                effectors.Add(e);
            }

            var a = new Encoder(effectors.ToArray());
            a.SequentialMerge();
            a.Encode("mp4");
        }
        */
        static void TestEffect()
        {
            var e = new Effector("1.jpeg", 24);
            e.Initialize();

            // test effects
            e.Earthquake(20, 2, 2);
            e.Shake(2, 1, 3);
            e.Rotate(90, false, 2);
            e.FullRotate(15, false, 2);
            e.Transition(400, 400, 5, 0, 0);

            e.Close();
        }

        /*static void TestRPC()
        {
            RpcServer rpcserver = new RpcServer(
                host: "mq.actoon.sokdak.me",
                user: "actoon_enc_server",
                password: "zhekfl12!"
            );

            rpcserver.Initialize();
            rpcserver.ListenQueue("rpc_encoding_queue", (model, ea) =>
            {
                var channel = (IModel)model;
                Console.WriteLine(ea.Body);
            });
        }*/
    }
}
