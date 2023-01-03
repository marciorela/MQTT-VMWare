using MQTT_Testes.Domain;
using MQTTnet;
using MQTTnet.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MQTT_Testes.Services
{
    public class MQTTService
    {
        private readonly IConfiguration _config;

        public MQTTService(IConfiguration configuration)
        {
            _config = configuration;
        }

        public async Task Register(Device device)
        {
            var mqttFactory = new MqttFactory();

            using var mqttClient = mqttFactory.CreateMqttClient();
            var mqttClientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(_config.GetValue<string>("MQTT:Server"))
                .WithCredentials(_config.GetValue<string>("MQTT:Username"), _config.GetValue<string>("MQTT:Password"))
                .Build();

            await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            var p = JsonSerializer.Serialize(device, options);
            var applicationMessage = new MqttApplicationMessageBuilder()
                .WithTopic($"{device.Topic()}/config")
                .WithPayload(p)
                .WithRetainFlag(false)
                .Build();

            await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);

            await mqttClient.DisconnectAsync();
        }

        public async Task UnRegister(Device device)
        {
            var mqttFactory = new MqttFactory();

            using var mqttClient = mqttFactory.CreateMqttClient();
            var mqttClientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(_config.GetValue<string>("MQTT:Server"))
                .WithCredentials(_config.GetValue<string>("MQTT:Username"), _config.GetValue<string>("MQTT:Password"))
                .Build();

            await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

            var applicationMessage = new MqttApplicationMessageBuilder()
                .WithTopic($"{device.Topic()}/config")
                .WithPayload("")
                .WithRetainFlag(false)
                .Build();

            await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);

            await mqttClient.DisconnectAsync();
        }

        public async Task SendState(Device device)
        {
            var mqttFactory = new MqttFactory();

            using var mqttClient = mqttFactory.CreateMqttClient();
            var mqttClientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(_config.GetValue<string>("MQTT:Server"))
                .WithCredentials(_config.GetValue<string>("MQTT:Username"), _config.GetValue<string>("MQTT:Password"))
                .Build();

            await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

            var applicationMessage = new MqttApplicationMessageBuilder()
                .WithTopic($"{device.Topic()}/state")
                .WithPayload(device.State() ? "ON" : "OFF")
                .Build();

            await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);

            await mqttClient.DisconnectAsync();
        }

    }
}
