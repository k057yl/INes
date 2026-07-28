namespace INest.Infrastructure.BackgroundServices.Reminder
{
    public class ReminderWorker : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<ReminderWorker> _logger;

        public ReminderWorker(IServiceProvider services, ILogger<ReminderWorker> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("[ReminderWorker] Запуск единого центра уведомлений.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var nowUtc = DateTime.UtcNow;

                    using (var scope = _services.CreateScope())
                    {
                        var reminderProcessor = scope.ServiceProvider.GetRequiredService<IReminderProcessor>();
                        await reminderProcessor.ProcessAsync(nowUtc, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[ReminderWorker] Критическая ошибка в цикле обработки.");
                }

                await Task.Delay(TimeSpan.FromMinutes(3), stoppingToken);
            }
        }
    }
}