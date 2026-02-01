using Colibri.WebApi.Services.Abstract;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Colibri.WebApi.Services
{
    public class DroneBoxBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DroneBoxBackgroundService> _logger;

        public DroneBoxBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<DroneBoxBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🚀 DroneBox Background Service started");

            // Ждем запуска приложения
            await Task.Delay(3000, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var statusService = scope.ServiceProvider.GetRequiredService<IDroneBoxStatusService>();
                    
                    await statusService.CheckDroneBoxStatusAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"❌ Error in DroneBox background check: {ex.Message}");
                }

                // Проверяем каждые 10 секунд
                await Task.Delay(10000, stoppingToken);
            }
        }
    }
}