using INest.Features.Telegram.Commands.ConnectTelegram;
using INest.Features.Telegram.Queries.SearchItems;
using MediatR;
using Microsoft.Extensions.Localization;
using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace INest.Infrastructure.Telegram
{
    public class TelegramBotBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TelegramBotBackgroundService> _logger;
        private readonly TelegramBotClient? _botClient;

        public TelegramBotBackgroundService(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            ILogger<TelegramBotBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;

            var token = configuration["TelegramBotSettings:BotToken"]
                        ?? Environment.GetEnvironmentVariable("TelegramBotSettings__BotToken");

            if (!string.IsNullOrWhiteSpace(token))
            {
                _botClient = new TelegramBotClient(token);
                _logger.LogInformation("[TG BOT] Клиент успешно инициализирован.");
            }
            else
            {
                _logger.LogError("[TG BOT] Токен бота НЕ НАЙДЕН в конфигурации!");
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_botClient == null)
            {
                _logger.LogWarning("Telegram BotToken отсутствует в конфигурации. Фоновый сервис не запущен.");
                return;
            }

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>(),
                DropPendingUpdates = true
            };

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var localizer = scope.ServiceProvider.GetRequiredService<IStringLocalizer<SharedResource>>();

                await _botClient.SetMyCommands(new[]
                {
                    new BotCommand { Command = "find", Description = localizer["TG_MENU_FIND"].Value },
                    new BotCommand { Command = "status", Description = localizer["TG_MENU_STATUS"].Value },
                    new BotCommand { Command = "help", Description = localizer["TG_MENU_HELP"].Value }
                }, cancellationToken: stoppingToken);

                _logger.LogInformation("[TG BOT] Кнопка меню команд успешно настроена.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[TG BOT] Не удалось настроить кнопку меню команд.");
            }

            _logger.LogInformation("Фоновый сервис Telegram Bot успешно запущен. Начинаем Long Polling...");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await _botClient.ReceiveAsync(
                        updateHandler: HandleUpdateAsync,
                        errorHandler: HandlePollingErrorAsync,
                        receiverOptions: receiverOptions,
                        cancellationToken: stoppingToken
                    );
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Критическая ошибка в цикле Long Polling Телеграм-бота. Перезапуск через 5 секунд...");
                    await Task.Delay(5000, stoppingToken);
                }
            }
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            if (update.Message is not { Text: { } messageText } message || message.Chat.Type != ChatType.Private)
                return;

            var chatId = message.Chat.Id;
            using var scope = _serviceProvider.CreateScope();
            var localizer = scope.ServiceProvider.GetRequiredService<IStringLocalizer<SharedResource>>();

            var userLang = message.From?.LanguageCode ?? "ru";
            var culture = new CultureInfo(userLang);
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;

            if (messageText == localizer["TG_BTN_FIND_ITEMS"].Value)
            {
                await botClient.SendMessage(chatId, localizer["TG_FIND_USAGE"].Value, parseMode: ParseMode.Markdown, cancellationToken: ct);
                return;
            }

            var parts = messageText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var command = parts[0].ToLower();

            if (command == "/start")
            {
                _logger.LogInformation("Получена команда /start от ChatId: {ChatId}", chatId);

                if (parts.Length < 2)
                {
                    await botClient.SendMessage(chatId, localizer["TG_WELCOME_WITHOUT_TOKEN"].Value, cancellationToken: ct);
                    return;
                }

                var token = parts[1];
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                var isLinked = await mediator.Send(new ConnectTelegramCommand(chatId, token), ct);

                if (isLinked)
                {
                    var replyKeyboard = new ReplyKeyboardMarkup(new[]
                    {
                        new KeyboardButton[] { localizer["TG_BTN_FIND_ITEMS"].Value }
                    })
                    {
                        ResizeKeyboard = true
                    };

                    await botClient.SendMessage(chatId, localizer["TG_LINK_SUCCESS"].Value, replyMarkup: replyKeyboard, cancellationToken: ct);
                }
                else
                {
                    await botClient.SendMessage(chatId, localizer["TG_LINK_ERROR"].Value, cancellationToken: ct);
                }
            }

            else if (command == "/find")
            {
                if (parts.Length < 2)
                {
                    await botClient.SendMessage(chatId, localizer["TG_FIND_USAGE"].Value, parseMode: ParseMode.Markdown, cancellationToken: ct);
                    return;
                }

                var searchTerm = string.Join(" ", parts[1..]);
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                var results = await mediator.Send(new SearchItemsQuery(chatId, searchTerm), ct);

                if (results == null || results.Count == 0)
                {
                    var emptyMsg = string.Format(localizer["TG_FIND_EMPTY"].Value, searchTerm);
                    await botClient.SendMessage(chatId, emptyMsg, parseMode: ParseMode.Markdown, cancellationToken: ct);
                    return;
                }

                var responseText = string.Format(localizer["TG_FIND_ITEM_HEADER"].Value, searchTerm);
                foreach (var item in results)
                {
                    responseText += string.Format(localizer["TG_FIND_ITEM_ROW"].Value, item.Name, item.StorageLocationName);
                    if (!string.IsNullOrWhiteSpace(item.Description))
                    {
                        responseText += string.Format(localizer["TG_FIND_ITEM_DESC"].Value, item.Description);
                    }
                    responseText += "\n-------------------\n\n";
                }

                await botClient.SendMessage(chatId, responseText, parseMode: ParseMode.Markdown, cancellationToken: ct);
            }

            else if (command == "/help")
            {
                await botClient.SendMessage(chatId, localizer["TG_HELP_TEXT"].Value, parseMode: ParseMode.Markdown, cancellationToken: ct);
            }

            else if (command == "/status")
            {
                await botClient.SendMessage(chatId, localizer["TG_STATUS_OK"].Value, cancellationToken: ct);
            }
        }

        private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken ct)
        {
            _logger.LogError(exception, "Ошибка Long Polling со стороны Telegram API.");
            return Task.CompletedTask;
        }
    }
}