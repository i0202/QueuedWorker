using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using WorkerService;

using IHost host = Host.CreateDefaultBuilder(args)
    .UseWindowsService()
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<MonitorLoop>();
        services.AddSingleton<DataInsertLoop>();
        services.AddHostedService<QueuedHostedService>();
        services.AddSingleton<IBackgroundTaskQueue>(_ =>
        {
            if (!int.TryParse(context.Configuration["QueueCapacity"], out var queueCapacity))
            {
                queueCapacity = 100;
            }

            return new DefaultBackgroundTaskQueue(queueCapacity);
        });
    })
    .Build();

MonitorLoop monitorLoop = host.Services.GetRequiredService<MonitorLoop>()!;
monitorLoop.StartMonitorLoop();
DataInsertLoop dataInsertLoop = host.Services.GetRequiredService<DataInsertLoop>()!;
//dataInsertLoop.StartInsertLoop();



await host.RunAsync();