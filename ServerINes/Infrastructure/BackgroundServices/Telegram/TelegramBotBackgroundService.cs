using INest.Data.Entities.Core;
using INest.Data.Enums;
using INest.Features.Telegram.Commands.ConnectTelegram;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace INest.Infrastructure.BackgroundServices.Telegram
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

            var token = configuration["Telegram:BotToken"]
                        ?? configuration["TelegramBotSettings:BotToken"]
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
                AllowedUpdates = new[] { UpdateType.Message, UpdateType.CallbackQuery },
                DropPendingUpdates = true
            };

            _logger.LogInformation("Фоновый сервис Telegram Bot успешно запущен. Начинаем Long Polling...");

            bool isCommandsConfigured = false;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (!isCommandsConfigured)
                    {
                        try
                        {
                            using var scope = _serviceProvider.CreateScope();
                            var localizer = scope.ServiceProvider.GetRequiredService<IStringLocalizer<SharedResource>>();

                            await _botClient.SetMyCommands(new[]
                            {
                                new BotCommand { Command = "find", Description = localizer["TG_MENU_FIND"].Value },
                                new BotCommand { Command = "stats", Description = localizer["TG_MENU_STATS"].Value },
                                new BotCommand { Command = "help", Description = localizer["TG_MENU_HELP"].Value }
                            }, cancellationToken: stoppingToken);

                            _logger.LogInformation("[TG BOT] Меню команд успешно настроено.");
                            isCommandsConfigured = true;
                        }
                        catch (Exception ex) when (IsNetworkError(ex))
                        {
                            _logger.LogWarning("[TG BOT] Не удалось настроить меню команд из-за отсутствия сети. Попробуем позже.");
                        }
                    }

                    await _botClient.ReceiveAsync(
                        updateHandler: HandleUpdateAsync,
                        errorHandler: HandlePollingErrorAsync,
                        receiverOptions: receiverOptions,
                        cancellationToken: stoppingToken
                    );
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    if (IsNetworkError(ex))
                    {
                        _logger.LogWarning("[TG BOT] Сеть недоступна. Повторная попытка подключения через 10 секунд...");
                        await Task.Delay(10000, stoppingToken);
                    }
                    else
                    {
                        _logger.LogError(ex, "[TG BOT] Критическая ошибка в цикле Long Polling. Перезапуск через 5 секунд...");
                        await Task.Delay(5000, stoppingToken);
                    }
                }
            }
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var localizer = scope.ServiceProvider.GetRequiredService<IStringLocalizer<SharedResource>>();

            var userLang = update.Message?.From?.LanguageCode ?? update.CallbackQuery?.From?.LanguageCode ?? "ru";
            var culture = new CultureInfo(userLang);
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;

            
            if (update.CallbackQuery is { } callback)
            {
                await HandleCallbackQueryAsync(botClient, callback, db, localizer, ct);
                return;
            }

            if (update.Message is not { Text: { } messageText } message || message.Chat.Type != ChatType.Private)
                return;

            var chatId = message.Chat.Id;
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
                        new KeyboardButton[] { localizer["TG_BTN_FIND_ITEMS"].Value, localizer["TG_BTN_STATS"].Value }
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
           
            else if (command == "/find" || messageText == localizer["TG_BTN_FIND_ITEMS"].Value)
            {
                if (parts.Length < 2 && command == "/find")
                {
                    await botClient.SendMessage(chatId, localizer["TG_FIND_USAGE"].Value, parseMode: ParseMode.Markdown, cancellationToken: ct);
                    return;
                }

                var searchTerm = command == "/find" ? string.Join(" ", parts[1..]) : "";

                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    await botClient.SendMessage(chatId, localizer["TG_FIND_USAGE"].Value, parseMode: ParseMode.Markdown, cancellationToken: ct);
                    return;
                }

                var user = await db.Users.FirstOrDefaultAsync(u => u.TelegramChatId == chatId, ct);
                if (user == null) return;

                var term = searchTerm.ToLower().Trim();
                var items = await db.Set<Item>()
                    .Include(i => i.StorageLocation)
                    .Include(i => i.Details)
                    .Where(i => i.User.Id == user.Id && i.Status != ItemStatus.Archived && i.Status != ItemStatus.Sold)
                    .Where(i => EF.Functions.Like(i.Name.ToLower(), $"%{term}%") || (i.Description != null && EF.Functions.Like(i.Description.ToLower(), $"%{term}%")))
                    .Take(5)
                    .ToListAsync(ct);

                if (items.Count == 0)
                {
                    var emptyMsg = string.Format(localizer["TG_FIND_EMPTY"].Value, searchTerm);
                    await botClient.SendMessage(chatId, emptyMsg, parseMode: ParseMode.Markdown, cancellationToken: ct);
                    return;
                }

                foreach (var item in items)
                {
                    var locationName = item.StorageLocation != null ? item.StorageLocation.Name : localizer["TG_FIND_ITEM_ROW"].Value;

                    var priceValue = item.Details?.PurchasePrice;
                    var currency = item.Details?.Currency ?? "USD";

                    var priceText = priceValue.HasValue
                        ? string.Format(localizer["TG_ITEM_PRICE"].Value, $"{priceValue.Value:N0} {currency}")
                        : localizer["TG_ITEM_PRICE_NOT_SET"].Value;

                    var caption = $"📦 *{item.Name}*\n" +
                                  string.Format(localizer["TG_FIND_ITEM_ROW"].Value, item.Name, locationName) + "\n" +
                                  (string.IsNullOrWhiteSpace(item.Description) ? "" : string.Format(localizer["TG_FIND_ITEM_DESC"].Value, item.Description) + "\n") +
                                  priceText;

                    var inlineButtons = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData(localizer["TG_BTN_WHERE"].Value, $"loc_{item.Id}"),
                            InlineKeyboardButton.WithCallbackData(localizer["TG_BTN_LEND"].Value, $"lend_{item.Id}")
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData(localizer["TG_BTN_SELL"].Value, $"sell_{item.Id}")
                        }
                    });

                    if (!string.IsNullOrWhiteSpace(item.PhotoUrl))
                    {
                        await botClient.SendPhoto(chatId, InputFile.FromUri(item.PhotoUrl), caption: caption, parseMode: ParseMode.Markdown, replyMarkup: inlineButtons, cancellationToken: ct);
                    }
                    else
                    {
                        await botClient.SendMessage(chatId, caption, parseMode: ParseMode.Markdown, replyMarkup: inlineButtons, cancellationToken: ct);
                    }
                }
            }
            
            else if (command == "/stats" || messageText == localizer["TG_BTN_STATS"].Value)
            {
                var user = await db.Users.FirstOrDefaultAsync(u => u.TelegramChatId == chatId, ct);
                if (user == null) return;

                var totalItems = await db.Set<Item>().CountAsync(i => i.User.Id == user.Id && i.Status != ItemStatus.Archived && i.Status != ItemStatus.Sold, ct);
                var totalPrice = await db.Set<Item>()
                    .Where(i => i.User.Id == user.Id && i.Status != ItemStatus.Archived && i.Status != ItemStatus.Sold)
                    .SumAsync(i => i.Details != null ? (i.Details.PurchasePrice ?? 0) : 0, ct);

                var lentItems = await db.Set<Item>()
                    .Where(i => i.User.Id == user.Id && i.Status == ItemStatus.Lent)
                    .Select(i => $"• *{i.Name}*")
                    .ToListAsync(ct);

                var statsText = $"{localizer["TG_STATS_HEADER"].Value}\n\n" +
                                $"{string.Format(localizer["TG_STATS_ITEMS_COUNT"].Value, totalItems)}\n" +
                                $"{string.Format(localizer["TG_STATS_TOTAL_PRICE"].Value, totalPrice.ToString("N0"))}\n\n" +
                                $"{string.Format(localizer["TG_STATS_LENT_TITLE"].Value, lentItems.Count)}\n" +
                                (lentItems.Count > 0 ? string.Join("\n", lentItems) : localizer["TG_STATS_LENT_EMPTY"].Value);

                await botClient.SendMessage(chatId, statsText, parseMode: ParseMode.Markdown, cancellationToken: ct);
            }
            else if (command == "/help")
            {
                await botClient.SendMessage(chatId, localizer["TG_HELP_TEXT"].Value, parseMode: ParseMode.Markdown, cancellationToken: ct);
            }
        }

        private async Task HandleCallbackQueryAsync(ITelegramBotClient botClient, CallbackQuery callback, AppDbContext db, IStringLocalizer<SharedResource> localizer, CancellationToken ct)
        {
            var data = callback.Data;
            if (string.IsNullOrEmpty(data)) return;

            var chatId = callback.Message!.Chat.Id;

            
            if (data.StartsWith("loc_"))
            {
                var itemId = Guid.Parse(data.Replace("loc_", ""));
                var item = await db.Set<Item>().Include(i => i.StorageLocation).FirstOrDefaultAsync(i => i.Id == itemId, ct);

                if (item != null)
                {
                    var path = new List<string>();
                    var currentLoc = item.StorageLocation;
                    while (currentLoc != null)
                    {
                        path.Insert(0, currentLoc.Name);
                        currentLoc = currentLoc.ParentLocationId.HasValue
                            ? await db.Set<StorageLocation>().FirstOrDefaultAsync(l => l.Id == currentLoc.ParentLocationId, ct)
                            : null;
                    }

                    var locationChain = path.Count > 0 ? string.Join(" ➔ ", path) : localizer["TG_FIND_ITEM_ROW"].Value;
                    await botClient.AnswerCallbackQuery(callback.Id, $"📍 {locationChain}", showAlert: true, cancellationToken: ct);
                }
            }
            
            else if (data.StartsWith("lend_"))
            {
                var itemId = Guid.Parse(data.Replace("lend_", ""));
                var item = await db.Set<Item>().FirstOrDefaultAsync(i => i.Id == itemId, ct);
                if (item != null)
                {
                    item.Lend();
                    await db.SaveChangesAsync(ct);

                    await botClient.AnswerCallbackQuery(callback.Id, localizer["TG_ALERT_LENT"].Value, cancellationToken: ct);
                    await botClient.SendMessage(chatId, string.Format(localizer["TG_LENT_SUCCESS"].Value, item.Name), parseMode: ParseMode.Markdown, cancellationToken: ct);
                }
            }
            
            else if (data.StartsWith("sell_"))
            {
                var itemId = Guid.Parse(data.Replace("sell_", ""));
                var item = await db.Set<Item>().FirstOrDefaultAsync(i => i.Id == itemId, ct);
                if (item != null)
                {
                    item.Sell();
                    await db.SaveChangesAsync(ct);

                    await botClient.AnswerCallbackQuery(callback.Id, localizer["TG_ALERT_SOLD"].Value, cancellationToken: ct);
                    await botClient.SendMessage(chatId, string.Format(localizer["TG_SOLD_SUCCESS"].Value, item.Name), parseMode: ParseMode.Markdown, cancellationToken: ct);
                }
            }

            else if (data.StartsWith("return_"))
            {
                var itemId = Guid.Parse(data.Replace("return_", ""));
                var item = await db.Set<Item>().FirstOrDefaultAsync(i => i.Id == itemId, ct);
                if (item != null)
                {
                    item.Return();
                    await db.SaveChangesAsync(ct);

                    await botClient.AnswerCallbackQuery(callback.Id, localizer["TG_ALERT_RETURNED"].Value, cancellationToken: ct);
                    await botClient.SendMessage(chatId, string.Format(localizer["TG_RETURNED_SUCCESS"].Value, item.Name), parseMode: ParseMode.Markdown, cancellationToken: ct);
                }
            }
            else if (data.StartsWith("extend_"))
            {
                await botClient.AnswerCallbackQuery(callback.Id, localizer["TG_ALERT_EXTENDED"].Value, showAlert: true, cancellationToken: ct);
            }
        }

        private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken ct)
        {
            if (IsNetworkError(exception))
            {
                _logger.LogWarning("[TG BOT] Ошибка сети при контакте с Telegram API: {Message}", exception.Message);
            }
            else
            {
                _logger.LogError(exception, "[TG BOT] Ошибка Long Polling со стороны Telegram API.");
            }

            return Task.CompletedTask;
        }

        private static bool IsNetworkError(Exception ex)
        {
            return ex is RequestException
                || ex is HttpRequestException
                || ex is System.Net.Sockets.SocketException
                || ex.InnerException is System.Net.Sockets.SocketException;
        }
    }
}