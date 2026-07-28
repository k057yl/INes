namespace INest.Infrastructure.BackgroundServices.Reminder
{
    public interface IReminderProcessor
    {
        Task ProcessAsync(DateTime nowUtc, CancellationToken stoppingToken);
    }
}
