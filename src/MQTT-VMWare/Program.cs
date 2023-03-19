using MQTT_Testes;
using MQTT_Testes.Services;
using MR.Log;
using MR.MQTT.Service;

//IHost host = Host.CreateDefaultBuilder(args)
//    .UseWindowsService(options =>
//    {
//        options.ServiceName = "MQTT VMWare - Virtual Machine Status for Home Assistant (by Rela)";
//    }
//    )
//    .ConfigureServices(services =>
//    {
//        services.AddSingleton<MQTTService>();
//        services.AddSingleton<VMService>();
//        services.AddHostedService<Worker>();
//    })
//    .Build();

IHost host = Host.CreateDefaultBuilder(args)
    .MRConfigureLogService()
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
    }).Build();

MRLog.ConfigureLogMain();

try
{
    await host.RunAsync();
}
finally
{
    MRLog.CloseAndFlush();
}
