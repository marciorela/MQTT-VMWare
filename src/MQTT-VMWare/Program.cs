using MQTT_Testes;
using MQTT_Testes.Services;

IHost host = Host.CreateDefaultBuilder(args)
    .UseWindowsService(options =>
    {
        options.ServiceName = "MQTT VMWare - Virtual Machine Status for Home Assistant (by Rela)";
    }
    )
    .ConfigureServices(services =>
    {
        services.AddSingleton<MQTTService>();
        services.AddSingleton<VMService>();
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
