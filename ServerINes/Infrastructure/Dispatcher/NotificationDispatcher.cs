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

            var token = configuration["Telegram:BotToken"]
                        ?? configuration["TelegramBotSettings:BotToken"]
                        ?? Environment.GetEnvironmentVariable("TelegramBotSettings__BotToken");

            if (!string.IsNullOrWhiteSpace(token))
            {
                _botClient = new TelegramBotClient(token);
            }
            else
            {
                _logger.LogWarning("[NotificationDispatcher] Токен Telegram бота не найден в конфигурации!");
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

            if (user == null)
            {
                _logger.LogWarning("[NotificationDispatcher] Пользователь с Id={UserId} не найден.", userId);
                return;
            }

            bool telegramSent = false;

            if (user.TelegramChatId.HasValue && user.TelegramChatId.Value != 0 && _botClient != null)
            {
                try
                {
                    _logger.LogInformation("[NotificationDispatcher] Отправка TG сообщения на ChatId: {ChatId}", user.TelegramChatId.Value);

                    await _botClient.SendMessage(
                        chatId: user.TelegramChatId.Value,
                        text: message,
                        cancellationToken: cancellationToken
                    );

                    telegramSent = true;
                    _logger.LogInformation("[NotificationDispatcher] Сообщение в TG успешно отправлено на ChatId: {ChatId}", user.TelegramChatId.Value);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[NotificationDispatcher] Ошибка отправки TG сообщения для ChatId: {ChatId}", user.TelegramChatId.Value);
                }
            }
            else
            {
                _logger.LogWarning("[NotificationDispatcher] Пропуск TG отправки: ChatId у юзера {UserId} отсутствует или _botClient не инициализирован.", userId);
            }

            if (!telegramSent && !string.IsNullOrEmpty(user.Email))
            {
                try
                {
                    _logger.LogInformation("[NotificationDispatcher] Отправка резервного Email на {Email}", user.Email);
                    await _emailService.SendEmailAsync(user.Email, emailSubjectKey, emailBodyKey);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[NotificationDispatcher] Ошибка отправки резервного письма на {Email}", user.Email);
                }
            }
        }
    }
}