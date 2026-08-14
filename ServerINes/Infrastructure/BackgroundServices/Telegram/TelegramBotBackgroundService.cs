using INest.Data.Entities.Core;
using ReminderEntity = INest.Data.Entities.Infrastructure.Reminder;
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
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_botClient == null) return;

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new[] { UpdateType.Message, UpdateType.CallbackQuery },
                DropPendingUpdates = true
            };

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
                                new BotCommand { Command = "where", Description = localizer["TG_MENU_WHERE"].Value },
                                new BotCommand { Command = "lent", Description = localizer["TG_MENU_LENT"].Value },
                                new BotCommand { Command = "reminders", Description = localizer["TG_MENU_REMINDERS"].Value },
                                new BotCommand { Command = "add", Description = localizer["TG_MENU_ADD"].Value },
                                new BotCommand { Command = "help", Description = localizer["TG_MENU_HELP"].Value }
                            }, cancellationToken: stoppingToken);

                            isCommandsConfigured = true;
                        }
                        catch (Exception ex) when (IsNetworkError(ex)) { }
                    }

                    await _botClient.ReceiveAsync(
                        updateHandler: HandleUpdateAsync,
                        errorHandler: HandlePollingErrorAsync,
                        receiverOptions: receiverOptions,
                        cancellationToken: stoppingToken
                    );
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
                catch (Exception) { await Task.Delay(5000, stoppingToken); }
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

            // 🎯 INLINE ACTIONS (Кнопки под сообщениями)
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

            // 1. СВЯЗЫВАНИЕ АККАУНТА (/start)
            if (command == "/start")
            {
                if (parts.Length < 2)
                {
                    await botClient.SendMessage(chatId, localizer["TG_WELCOME_WITHOUT_TOKEN"].Value, cancellationToken: ct);
                    return;
                }

                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                var isLinked = await mediator.Send(new ConnectTelegramCommand(chatId, parts[1]), ct);

                if (isLinked)
                {
                    var replyKeyboard = new ReplyKeyboardMarkup(new[]
                    {
                        new KeyboardButton[] { localizer["TG_BTN_FIND_ITEMS"].Value, localizer["TG_BTN_LENT_ITEMS"].Value },
                        new KeyboardButton[] { localizer["TG_BTN_REMINDERS"].Value, localizer["TG_BTN_ADD_ITEM"].Value }
                    })
                    { ResizeKeyboard = true };

                    await botClient.SendMessage(chatId, localizer["TG_LINK_SUCCESS"].Value, replyMarkup: replyKeyboard, cancellationToken: ct);
                }
                else
                {
                    await botClient.SendMessage(chatId, localizer["TG_LINK_ERROR"].Value, cancellationToken: ct);
                }
            }
            // 2. ПОИСК ВЕЩЕЙ И ДЕЙСТВИЯ (/find)
            else if (command == "/find" || messageText == localizer["TG_BTN_FIND_ITEMS"].Value)
            {
                if (parts.Length < 2 && command == "/find")
                {
                    await botClient.SendMessage(chatId, localizer["TG_FIND_USAGE"].Value, parseMode: ParseMode.Markdown, cancellationToken: ct);
                    return;
                }

                var term = (command == "/find" ? string.Join(" ", parts[1..]) : "").ToLower().Trim();
                if (string.IsNullOrWhiteSpace(term))
                {
                    await botClient.SendMessage(chatId, localizer["TG_FIND_USAGE"].Value, parseMode: ParseMode.Markdown, cancellationToken: ct);
                    return;
                }

                var user = await db.Users.FirstOrDefaultAsync(u => u.TelegramChatId == chatId, ct);
                if (user == null) return;

                var items = await db.Set<Item>()
                    .Include(i => i.StorageLocation)
                    .Include(i => i.Details)
                    .Where(i => i.User.Id == user.Id && i.Status != ItemStatus.Archived && i.Status != ItemStatus.Sold)
                    .Where(i => EF.Functions.Like(i.Name.ToLower(), $"%{term}%"))
                    .Take(5)
                    .ToListAsync(ct);

                if (items.Count == 0)
                {
                    await botClient.SendMessage(chatId, string.Format(localizer["TG_FIND_EMPTY"].Value, term), parseMode: ParseMode.Markdown, cancellationToken: ct);
                    return;
                }

                foreach (var item in items)
                {
                    var chain = await GetLocationChainAsync(db, item.StorageLocation, localizer, ct);
                    var priceStr = item.Details?.PurchasePrice.HasValue == true
                        ? $"💰 {item.Details.PurchasePrice.Value:N0} {item.Details.Currency ?? "UAH"}"
                        : "";

                    var caption = $"📦 *{item.Name}*\n" +
                                  $"📍 {chain}\n" +
                                  (string.IsNullOrEmpty(priceStr) ? "" : $"{priceStr}\n");

                    var inlineButtons = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData(localizer["TG_BTN_WHERE"].Value, $"where_{item.Id}"),
                            InlineKeyboardButton.WithCallbackData(localizer["TG_BTN_LEND"].Value, $"lend_{item.Id}"),
                            InlineKeyboardButton.WithCallbackData(localizer["TG_BTN_SELL"].Value, $"sell_{item.Id}")
                        }
                    });

                    await botClient.SendMessage(chatId, caption, parseMode: ParseMode.Markdown, replyMarkup: inlineButtons, cancellationToken: ct);
                }
            }
            // 3. ЦЕПОЧКА ЛОКАЦИЙ (/where)
            else if (command == "/where")
            {
                if (parts.Length < 2)
                {
                    await botClient.SendMessage(chatId, localizer["TG_WHERE_USAGE"].Value, parseMode: ParseMode.Markdown, cancellationToken: ct);
                    return;
                }

                var term = string.Join(" ", parts[1..]).ToLower().Trim();
                var user = await db.Users.FirstOrDefaultAsync(u => u.TelegramChatId == chatId, ct);
                if (user == null) return;

                var items = await db.Set<Item>()
                    .Include(i => i.StorageLocation)
                    .Where(i => i.User.Id == user.Id && i.Status == ItemStatus.Active)
                    .Where(i => EF.Functions.Like(i.Name.ToLower(), $"%{term}%"))
                    .Take(5)
                    .ToListAsync(ct);

                if (items.Count == 0)
                {
                    await botClient.SendMessage(chatId, string.Format(localizer["TG_FIND_EMPTY"].Value, term), parseMode: ParseMode.Markdown, cancellationToken: ct);
                    return;
                }

                foreach (var item in items)
                {
                    var chain = await GetLocationChainAsync(db, item.StorageLocation, localizer, ct);
                    await botClient.SendMessage(chatId, $"📦 *{item.Name}*\n📍 {chain}", parseMode: ParseMode.Markdown, cancellationToken: ct);
                }
            }
            // 4. ОДОЛЖЕННЫЕ ВЕЩИ (/lent)
            else if (command == "/lent" || messageText == localizer["TG_BTN_LENT_ITEMS"].Value)
            {
                var user = await db.Users.FirstOrDefaultAsync(u => u.TelegramChatId == chatId, ct);
                if (user == null) return;

                var lentItems = await db.Set<Item>()
                    .Include(i => i.Reminders)
                    .Where(i => i.User.Id == user.Id && i.Status == ItemStatus.Lent)
                    .ToListAsync(ct);

                if (lentItems.Count == 0)
                {
                    await botClient.SendMessage(chatId, localizer["TG_LENT_EMPTY"].Value, parseMode: ParseMode.Markdown, cancellationToken: ct);
                    return;
                }

                await botClient.SendMessage(chatId, string.Format(localizer["TG_LENT_HEADER"].Value, lentItems.Count), parseMode: ParseMode.Markdown, cancellationToken: ct);

                foreach (var item in lentItems)
                {
                    var activeReminder = item.Reminders.FirstOrDefault(r => !r.IsCompleted);
                    var dueDateStr = activeReminder != null ? activeReminder.TriggerAt.ToString("dd.MM.yyyy") : localizer["TG_DATE_NOT_SET"].Value;

                    var caption = $"🔧 *{item.Name}*\n📅 До: *{dueDateStr}*";

                    var buttons = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData(localizer["TG_BTN_RETURNED"].Value, $"return_{item.Id}"),
                            InlineKeyboardButton.WithCallbackData(localizer["TG_BTN_EXTEND"].Value, $"extend_{item.Id}")
                        }
                    });

                    await botClient.SendMessage(chatId, caption, parseMode: ParseMode.Markdown, replyMarkup: buttons, cancellationToken: ct);
                }
            }
            // 5. НАПОМИНАНИЯ (/reminders)
            else if (command == "/reminders" || messageText == localizer["TG_BTN_REMINDERS"].Value)
            {
                var user = await db.Users.FirstOrDefaultAsync(u => u.TelegramChatId == chatId, ct);
                if (user == null) return;

                var reminders = await db.Set<ReminderEntity>()
                    .Include(r => r.Item)
                    .Where(r => r.Item.User.Id == user.Id && !r.IsCompleted && r.TriggerAt >= DateTime.UtcNow.AddDays(-1))
                    .OrderBy(r => r.TriggerAt)
                    .Take(5)
                    .ToListAsync(ct);

                if (reminders.Count == 0)
                {
                    await botClient.SendMessage(chatId, localizer["TG_REMINDERS_EMPTY"].Value, parseMode: ParseMode.Markdown, cancellationToken: ct);
                    return;
                }

                var text = localizer["TG_REMINDERS_HEADER"].Value + "\n\n";
                foreach (var r in reminders)
                {
                    var itemName = r.Item != null ? $" ({r.Item.Name})" : "";
                    text += $"• *{r.TriggerAt:dd.MM.yyyy}* — {r.Title}{itemName}\n";
                }

                await botClient.SendMessage(chatId, text, parseMode: ParseMode.Markdown, cancellationToken: ct);
            }
            // 6. БЫСТРОЕ ДОБАВЛЕНИЕ (/add <Название>)
            else if (command == "/add" || messageText == localizer["TG_BTN_ADD_ITEM"].Value)
            {
                if (parts.Length < 2)
                {
                    await botClient.SendMessage(chatId, localizer["TG_ADD_USAGE"].Value, parseMode: ParseMode.Markdown, cancellationToken: ct);
                    return;
                }

                var itemName = string.Join(" ", parts[1..]).Trim();
                var user = await db.Users.FirstOrDefaultAsync(u => u.TelegramChatId == chatId, ct);
                if (user == null) return;

                var defaultCategory = await db.Set<Category>().FirstOrDefaultAsync(c => c.User.Id == user.Id, ct);
                if (defaultCategory == null)
                {
                    await botClient.SendMessage(chatId, "❌ Сначала создайте хотя бы одну категорию на сайте.", cancellationToken: ct);
                    return;
                }

                var newItem = new Item
                {
                    Name = itemName,
                    User = user,
                    Category = defaultCategory
                };

                db.Set<Item>().Add(newItem);
                await db.SaveChangesAsync(ct);

                await botClient.SendMessage(chatId, $"✅ Вещь «*{newItem.Name}*» добавлена на склад!", parseMode: ParseMode.Markdown, cancellationToken: ct);
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

            if (data.StartsWith("where_"))
            {
                var itemId = Guid.Parse(data.Replace("where_", ""));
                var item = await db.Set<Item>().Include(i => i.StorageLocation).FirstOrDefaultAsync(i => i.Id == itemId, ct);
                if (item != null)
                {
                    var chain = await GetLocationChainAsync(db, item.StorageLocation, localizer, ct);
                    await botClient.AnswerCallbackQuery(callback.Id, $"📍 {chain}", showAlert: true, cancellationToken: ct);
                }
            }
            else if (data.StartsWith("sell_"))
            {
                var itemId = Guid.Parse(data.Replace("sell_", ""));
                var item = await db.Set<Item>().FirstOrDefaultAsync(i => i.Id == itemId, ct);
                if (item != null)
                {
                    var confirmButtons = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData(localizer["TG_BTN_CONFIRM_SELL"].Value, $"confirm_sell_{item.Id}"),
                            InlineKeyboardButton.WithCallbackData(localizer["TG_BTN_CANCEL"].Value, $"cancel_action")
                        }
                    });

                    await botClient.SendMessage(chatId, string.Format(localizer["TG_CONFIRM_SELL"].Value, item.Name), parseMode: ParseMode.Markdown, replyMarkup: confirmButtons, cancellationToken: ct);
                }
            }
            else if (data.StartsWith("confirm_sell_"))
            {
                var itemId = Guid.Parse(data.Replace("confirm_sell_", ""));
                var item = await db.Set<Item>().FirstOrDefaultAsync(i => i.Id == itemId, ct);
                if (item != null)
                {
                    item.Sell(); // Вызов доменного метода
                    await db.SaveChangesAsync(ct);

                    await botClient.AnswerCallbackQuery(callback.Id, "Продано!", cancellationToken: ct);
                    await botClient.SendMessage(chatId, string.Format(localizer["TG_SOLD_SUCCESS"].Value, item.Name), parseMode: ParseMode.Markdown, cancellationToken: ct);
                }
            }
            else if (data.StartsWith("return_"))
            {
                var itemId = Guid.Parse(data.Replace("return_", ""));
                var item = await db.Set<Item>().FirstOrDefaultAsync(i => i.Id == itemId, ct);
                if (item != null)
                {
                    item.Return(); // Вызов доменного метода
                    await db.SaveChangesAsync(ct);

                    await botClient.AnswerCallbackQuery(callback.Id, "Вернулось!", cancellationToken: ct);
                    await botClient.SendMessage(chatId, string.Format(localizer["TG_RETURNED_SUCCESS"].Value, item.Name), parseMode: ParseMode.Markdown, cancellationToken: ct);
                }
            }
            else if (data == "cancel_action")
            {
                await botClient.AnswerCallbackQuery(callback.Id, localizer["TG_ACTION_CANCELLED"].Value, cancellationToken: ct);
            }
        }

        private async Task<string> GetLocationChainAsync(AppDbContext db, StorageLocation? location, IStringLocalizer<SharedResource> localizer, CancellationToken ct)
        {
            if (location == null) return localizer["TG_NO_LOCATION"].Value;

            var path = new List<string>();
            var currentLoc = location;
            while (currentLoc != null)
            {
                path.Insert(0, currentLoc.Name);
                currentLoc = currentLoc.ParentLocationId.HasValue
                    ? await db.Set<StorageLocation>().FirstOrDefaultAsync(l => l.Id == currentLoc.ParentLocationId, ct)
                    : null;
            }

            return string.Join(" ➔ ", path);
        }

        private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken ct)
        {
            return Task.CompletedTask;
        }

        private static bool IsNetworkError(Exception ex)
        {
            return ex is RequestException || ex is HttpRequestException;
        }
    }
}