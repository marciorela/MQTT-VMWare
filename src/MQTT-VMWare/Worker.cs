using MQTTnet.Client;
using MQTTnet;
using System.Text.Json;
using MQTT_Testes.Domain;
using MQTT_Testes.Services;

namespace MQTT_Testes
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _config;
        private readonly VMService _vmService;
        private readonly MQTTService _mqtt;

        public Worker(ILogger<Worker> logger, IConfiguration config, VMService vmService, MQTTService mqtt)
        {
            _logger = logger;
            _config = config;
            _vmService = vmService;
            _mqtt = mqtt;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var _delay = _config.GetValue<int>("Service:Delay", 30000);

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                var running = _vmService.ListRunningVMs();
                //var devices = new List<Device>();
                var vms = _vmService.ListAllVMs();
                foreach (var vm in vms)
                {
                    var device = new Device(vm, _mqtt);

                    device.SetState(running.Where(x => x == device.Name).Any());
                    //await _mqtt.UnRegister(device);

                    //devices.Add(device);
                }

                await Task.Delay(_delay, stoppingToken);
            }
        }
    }
}