using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using MQTTnet.Protocol;

namespace Test
{
    class MqttClient
    {
        public static class ClientTest
        {

            public class RandomPassword : IMqttClientCredentials
            {
                public byte[] Password => Encoding.UTF8.GetBytes("PASS");

                public string Username => "USER";
            }

            public static async Task RunAsync()
            {
                try
                {

                    var factory = new MqttFactory();
                    var client = factory.CreateMqttClient();
                    var clientOptions = new MqttClientOptions
                    {
                        ChannelOptions = new MqttClientTcpOptions
                        {
                            Server = "127.0.0.1"
                        },
                        Credentials = new RandomPassword()
                    };

                    client.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(e =>
                    {
                        Console.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
                        Console.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
                        Console.WriteLine($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
                        Console.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
                        Console.WriteLine($"+ Retain = {e.ApplicationMessage.Retain}");
                        Console.WriteLine();
                    });

                    client.ConnectedHandler = new MqttClientConnectedHandlerDelegate(async e =>
                    {
                        Console.WriteLine("### CONNECTED WITH SERVER ###");

                        await client.SubscribeAsync(new TopicFilterBuilder().WithTopic("#").Build());

                        Console.WriteLine("### SUBSCRIBED ###");
                    });

                    client.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(async e =>
                    {
                        Console.WriteLine("### DISCONNECTED FROM SERVER ###");
                        await Task.Delay(TimeSpan.FromSeconds(5));

                        try
                        {
                            await client.ConnectAsync(clientOptions);
                        }
                        catch
                        {
                            Console.WriteLine("### RECONNECTING FAILED ###");
                        }
                    });

                    try
                    {
                        await client.ConnectAsync(clientOptions);
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine("### CONNECTING FAILED ###" + Environment.NewLine + exception);
                    }

                    Console.WriteLine("### WAITING FOR APPLICATION MESSAGES ###");

                    while (true)
                    {
                        Console.ReadLine();

                        await client.SubscribeAsync(new TopicFilter
                            {Topic = "test", QualityOfServiceLevel = MqttQualityOfServiceLevel.AtMostOnce});

                        var applicationMessage = new MqttApplicationMessageBuilder()
                            .WithTopic("test")
                            .WithPayload("Hello World")
                            .WithAtLeastOnceQoS()
                            .Build();

                        await client.PublishAsync(applicationMessage);
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }
            }
        }
    }
}
