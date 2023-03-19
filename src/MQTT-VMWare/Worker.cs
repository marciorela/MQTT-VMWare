using MQTTnet.Client;
using MQTTnet;
using System.Text.Json;
using MQTT_Testes.Domain;
using MQTT_Testes.Services;
using MR.MQTT.Service;
using MR.MQTT.Domain;

namespace MQTT_Testes
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _config;
        private readonly VMService _vmService;
        private readonly MQTTService _mqtt;
        private List<Device> _devices = new();

        public Worker(ILogger<Worker> logger, IConfiguration config, VMService vmService, MQTTService mqtt)
        {
            _logger = logger;
            _config = config;
            _vmService = vmService;
            _mqtt = mqtt;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

            var _delay = _config.GetValue<int>("Service:Delay", 30000);

            while (!stoppingToken.IsCancellationRequested)
            {

                var running = _vmService.ListRunningVMs();
                //var devices = new List<Device>();
                var vms = _vmService.ListAllVMs();
                foreach (var vm in vms)
                {
                    var device = GetDevice(vm);

                    await _mqtt.Register(device);
                    await _mqtt.SendState(device, running.Where(x => x == device.Name).Any());
                    //device.SetState(running.Where(x => x == device.Name).Any());

                    if (!_devices.Where(x => x.Name == device.Name).Any())
                    {
                        _devices.Add(device);
                    }
                }

                await Task.Delay(_delay, stoppingToken);
            }
        }

        private Device GetDevice(string folder)
        {
            var device = _devices.FirstOrDefault(x => x.Name == folder);
            if (device == null)
            {
                device = new Device(folder);
                _devices.Add(device);
            }
            return device;

        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            foreach (var device in _devices)
            {
                await _mqtt.UnRegister(device);
            }
        }
    }
}