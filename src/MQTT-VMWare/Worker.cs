using MQTTnet.Client;
using MQTTnet;
using System.Text.Json;
using MQTT_Testes.Domain;
using MQTT_Testes.Services;
using MR.MQTT.Service;
using MR.MQTT.Domain;
using System.Linq;

namespace MQTT_Testes
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _config;
        private readonly VMService _vmService;
        private readonly MQTTService _mqtt;
        private readonly List<Device> _devices = new();

        public Worker(ILogger<Worker> logger, IConfiguration config, VMService vmService, MQTTService mqtt)
        {
            _logger = logger;
            _config = config;
            _vmService = vmService;
            _mqtt = mqtt;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                var _delay = _config.GetValue<int>("Service:Delay", 30000);

                while (!stoppingToken.IsCancellationRequested)
                {

                    var running = _vmService.ListRunningVMs();
                    var vms = _vmService.ListAllVMs();
                    foreach (var vm in vms)
                    {
                        var device = GetDevice(vm);

                        var run = running.Where(x => x.Name == device.Id).FirstOrDefault();
                        if (run != null)
                        {
                            device.Attributes = run;
                        }

                        await _mqtt.Register(device);
                        await _mqtt.SendState(device, run != null);
                    }

                    await Task.Delay(_delay, stoppingToken);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }
        }

        private Device GetDevice(string id)
        {
            var device = _devices.FirstOrDefault(x => x.Id == id);
            if (device == null)
            {
                device = new Device(id);
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