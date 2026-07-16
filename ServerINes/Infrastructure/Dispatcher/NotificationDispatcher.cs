using INest.Infrastructure.Email;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Notification = INest.Data.Entities.Infrastructure.Notification;
using Task = System.Threading.Tasks.Task;

namespace INest.Infrastructure.Dispatcher
{
    public class NotificationDispatcher : INotificationDispatcher
    {
        private readonly AppDbContext _db;
        private readonly IEmailService _emailService;
        private readonly ILogger<NotificationDispatcher> _logger;
        private readonly TelegramBotClient? _botClient;

        public NotificationDispatcher(
            AppDbContext db,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<NotificationDispatcher> logger)
        {
            _db = db;
            _emailService = emailService;
            _logger = logger;

            var token = configuration["Telegram:BotToken"];
            if (!string.IsNullOrWhiteSpace(token))
            {
                _botClient = new TelegramBotClient(token);
            }
        }

        public async Task SendAsync(Guid userId, string message, string emailSubjectKey, string emailBodyKey, CancellationToken cancellationToken)
        {
            var notification = new Notification
            {
                UserId = userId,
                Message = message,
                IsRead = false
            };

            await _db.Notifications.AddAsync(notification, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            var user = await _db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user == null || string.IsNullOrEmpty(user.Email)) return;

            if (user.TelegramChatId.HasValue && _botClient != null)
            {
                try
                {
                    await _botClient.SendMessage(
                        chatId: user.TelegramChatId.Value,
                        text: message,
                        cancellationToken: cancellationToken
                    );
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Не удалось отправить TG пуш для ChatId: {ChatId}. Переключаемся на Email.", user.TelegramChatId.Value);
                }
            }

            try
            {
                await _emailService.SendEmailAsync(user.Email, emailSubjectKey, emailBodyKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка отправки резервного письма на {Email}", user.Email);
            }
        }
    }
}