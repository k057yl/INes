using INest.Data.Entities.Infrastructure;
using Microsoft.AspNetCore.Identity;

namespace INest.Infrastructure.BackgroundServices.Cleanup
{
    public class UnconfirmedUserCleanupWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<UnconfirmedUserCleanupWorker> _logger;

        public UnconfirmedUserCleanupWorker(IServiceProvider serviceProvider, ILogger<UnconfirmedUserCleanupWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("UnconfirmedUserCleanupWorker запущен.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var cleanupService = scope.ServiceProvider.GetRequiredService<UnconfirmedUserCleanupService>();

                    await cleanupService.CleanupAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка в процессе фоновой очистки неподтвержденных пользователей.");
                }

                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }
}