using Newtonsoft.Json;
using OpenCvSharp;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Util;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;
using System.Net.Http;
using System.Net;
using System.IO.Compression;
using System.IO;
using kornsalad.EffectParameters;

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

                try
                {
                    Convert.FromBase64String(Encoding.UTF8.GetString(ea.Body));
                }
                catch
                {
                    Console.WriteLine(" [!] not a message");
                    return;
                }

                var prop = ea.BasicProperties;
                var replyProp = channel.CreateBasicProperties();
                replyProp.CorrelationId = ea.BasicProperties.CorrelationId;
                
                var decodedB64 = Encoding.UTF8.GetString(Convert.FromBase64String(Encoding.UTF8.GetString(ea.Body)));
                var rpcRequest = JsonConvert.DeserializeObject<RpcRequest>(decodedB64);

                var file_url_const = "https://actoon.sokdak.me/media/{0}";

                var baseImage = new List<Task>();
                var appendImage = new List<Task>();

                // business logics
                foreach (Cut c in rpcRequest.cuts)
                {
                    // find a task that has a current cut
                    var curTask = rpcRequest.tasks.Where((f) => f.cut_file == c.file).ToArray();

                    // get cut file from backend
                    var cutUrl = string.Format(file_url_const, c.file);
                    
                    try
                    {
                        using (var webClient = new WebClient())
                        {
                            webClient.DownloadFile(cutUrl, c.file);
                            Console.WriteLine(" [x] Download completed from backend: {0}", c.file);
                        }
                    }
                    catch
                    {
                        Console.WriteLine(" [!] cannot get file {0}", c.file);
                    }

                    if (curTask.Count() < 1)
                    {
                        var newTask = new Task
                        {
                            cut_file = c.file,
                            effect_name = "appear",
                            parameters = "{'time': 3}",
                            cut_pos_x = c.pos_x,
                            cut_pos_y = c.pos_y
                        };

                        if (c.type == "SC")
                            baseImage.Add(newTask);
                        else appendImage.Add(newTask);
                    }
                    else
                    {
                        curTask[0].cut_pos_x = c.pos_x;
                        curTask[0].cut_pos_y = c.pos_y;

                        if (c.type == "SC")
                            baseImage.AddRange(curTask);
                        else appendImage.AddRange(curTask);
                    }
                }
                
                // initialize layer
                var e = new Effector(30, new Size(
                    Int32.Parse(appendImage[0].project_resolution_width),
                    Int32.Parse(appendImage[0].project_resolution_height)));

                e.Initialize("white");
                
                // do effect with base layers
                foreach (var task in baseImage)
                    DoEffectByEffectName(e, task);

                // do effect with appended layers
                foreach (var task in appendImage)
                    DoEffectByEffectName(e, task);

                // get results
                string resultFileName = e.Encode();

                var response = new RpcResponse
                {
                    result = "success",
                    file = Convert.ToBase64String(File.ReadAllBytes(resultFileName))
                };

                string result = JsonConvert.SerializeObject(response);

                // end of logics
                channel.BasicPublish(
                    exchange: "", 
                    routingKey: prop.ReplyTo,
                    basicProperties: replyProp,
                    body: Encoding.UTF8.GetBytes(result)
                );

                channel.BasicAck(
                    deliveryTag: ea.DeliveryTag,
                    multiple: false
                );
            });
        }
        
        private static void DoEffectByEffectName(Effector effector, Task task)
        {
            if (task.cut_pos_x != null && task.cut_pos_y != null)
                effector.AddLayer(task.cut_file,
                    Int32.Parse(task.cut_pos_x), Int32.Parse(task.cut_pos_y));
            else
                effector.AddLayer(task.cut_file); // will produces centered image

            var effectToBeParsed = task.parameters;
            
            switch (task.effect_name)
            {
                case "earthquake":
                    var param = JsonConvert.DeserializeObject<Earthquake>(task.parameters);
                    Console.WriteLine(" [i] make earthquake with param [power: {0}, speed: {1}, time: {2}]",
                        param.power, param.speed, param.time);
                    effector.Earthquake(param.power, param.speed, param.time);
                    break;
                case "appear":
                    var aparam = JsonConvert.DeserializeObject<Appear>(task.parameters);
                    Console.WriteLine(" [i] make appear with param [time: {0}]",
                        aparam.time);
                    effector.None(aparam.time);
                    break;
                case "shake":
                    var sparam = JsonConvert.DeserializeObject<Shake>(task.parameters);
                    Console.WriteLine(" [i] make shake with param [degree: {0}, speed: {1}, count: {2}]",
                       sparam.degree, sparam.speed, sparam.count);
                    effector.Shake(sparam.degree, sparam.speed, sparam.count);
                    break;
                case "rotate":
                    var rparam = JsonConvert.DeserializeObject<Rotate>(task.parameters);
                    Console.WriteLine(" [i] make rotate with param [degree: {0}, way: {1}, speed: {2}]",
                       rparam.degree, rparam.way ? "clockwise" : "counter-clockwise", rparam.speed);
                    effector.Rotate(rparam.degree, rparam.way, rparam.speed);
                    break;
                case "transition":
                    var tparam = JsonConvert.DeserializeObject<Transition>(task.parameters);
                    Console.WriteLine(" [i] make transition with param [xdes,ydes: ({0},{1}), speed: {2}, xpos,ypos: ({3},{4})]",
                       tparam.xdes, tparam.ydes, tparam.speed, tparam.xpos, tparam.ypos);
                    effector.Transition(tparam.xdes, tparam.ydes, tparam.speed, tparam.xpos, tparam.ypos);
                    break;
                default:
                    Console.WriteLine(" [!] not supported effect {0}, fallback to appear with 3 seconds",
                        task.effect_name);
                    goto case "appear";
            }
        }
    }
}
