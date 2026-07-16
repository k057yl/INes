namespace INest.Infrastructure.Dispatcher
{
    public interface INotificationDispatcher
    {
        Task SendAsync(Guid userId, string message, string emailSubjectKey, string emailBodyKey, CancellationToken cancellationToken);
    }
}
