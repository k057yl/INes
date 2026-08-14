using INest.Data.Entities.Core;
using INest.Data.Entities.Finances;
using INest.Data.Enums;
using INest.Features.Items.Commands.CreateItem;
using INest.Features.Items.DTOs;
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
                                new BotCommand { Command = "locations", Description = localizer["TG_MENU_LOCATIONS"].Value },
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
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
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

            // Создаем основную клавиатуру из 3 кнопок
            var mainReplyKeyboard = GetMainReplyKeyboard(localizer);

            if (command == "/start")
            {
                if (parts.Length < 2)
                {
                    await botClient.SendMessage(chatId, localizer["TG_WELCOME_WITHOUT_TOKEN"].Value, cancellationToken: ct);
                    return;
                }

                var isLinked = await mediator.Send(new ConnectTelegramCommand(chatId, parts[1]), ct);

                if (isLinked)
                {
                    await botClient.SendMessage(chatId, localizer["TG_LINK_SUCCESS"].Value, replyMarkup: mainReplyKeyboard, cancellationToken: ct);
                }
                else
                {
                    await botClient.SendMessage(chatId, localizer["TG_LINK_ERROR"].Value, cancellationToken: ct);
                }
                return;
            }

            var userId = await GetUserIdByChatIdAsync(db, chatId, ct);
            if (userId == Guid.Empty) return;

            // 1. ПОИСК (/find или кнопка 🔎 Найти вещь)
            if (command == "/find" || messageText == localizer["TG_BTN_FIND_ITEMS"].Value)
            {
                var term = (command == "/find" ? string.Join(" ", parts[1..]) : string.Empty).ToLower().Trim();

                if (string.IsNullOrWhiteSpace(term))
                {
                    await botClient.SendMessage(chatId, localizer["TG_FIND_USAGE"].Value, parseMode: ParseMode.Markdown, replyMarkup: mainReplyKeyboard, cancellationToken: ct);
                    return;
                }

                var items = await db.Set<Item>()
                    .Include(i => i.StorageLocation)
                    .Where(i => i.UserId == userId && i.Status == ItemStatus.Active && i.Name.ToLower().Contains(term))
                    .Take(5)
                    .ToListAsync(ct);

                if (!items.Any())
                {
                    await botClient.SendMessage(chatId, string.Format(localizer["TG_FIND_EMPTY"].Value, term), parseMode: ParseMode.Markdown, replyMarkup: mainReplyKeyboard, cancellationToken: ct);
                    return;
                }

                foreach (var item in items)
                {
                    var chain = await BuildLocationChainInlineAsync(db, item.StorageLocationId, localizer, ct);
                    var caption = $"📦 *{item.Name}*\n📍 {chain}";

                    var buttons = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData(localizer["TG_BTN_WHERE_IS_IT"].Value, $"where_{item.Id}"),
                            InlineKeyboardButton.WithCallbackData(localizer["TG_BTN_MOVE"].Value, $"move_{item.Id}")
                        }
                    });

                    await botClient.SendMessage(chatId, caption, parseMode: ParseMode.Markdown, replyMarkup: buttons, cancellationToken: ct);
                }
            }
            // 2. ГДЕ ЛЕЖИТ (/where)
            else if (command == "/where")
            {
                var term = string.Join(" ", parts[1..]).ToLower().Trim();
                if (string.IsNullOrWhiteSpace(term))
                {
                    await botClient.SendMessage(chatId, localizer["TG_WHERE_USAGE"].Value, parseMode: ParseMode.Markdown, replyMarkup: mainReplyKeyboard, cancellationToken: ct);
                    return;
                }

                var items = await db.Set<Item>()
                    .Where(i => i.UserId == userId && i.Status == ItemStatus.Active && i.Name.ToLower().Contains(term))
                    .Take(5)
                    .ToListAsync(ct);

                if (!items.Any())
                {
                    await botClient.SendMessage(chatId, string.Format(localizer["TG_FIND_EMPTY"].Value, term), parseMode: ParseMode.Markdown, replyMarkup: mainReplyKeyboard, cancellationToken: ct);
                    return;
                }

                if (items.Count == 1)
                {
                    var item = items.First();
                    var tree = await BuildLocationTreeAsync(db, item.StorageLocationId, localizer, ct);
                    await botClient.SendMessage(chatId, $"📦 *{item.Name}*\n\n{tree}", parseMode: ParseMode.Markdown, replyMarkup: mainReplyKeyboard, cancellationToken: ct);
                }
                else
                {
                    var buttons = items.Select(i => new[] { InlineKeyboardButton.WithCallbackData($"🔧 {i.Name}", $"where_{i.Id}") });
                    var markup = new InlineKeyboardMarkup(buttons);
                    await botClient.SendMessage(chatId, string.Format(localizer["TG_WHERE_FOUND_MULTIPLE"].Value, items.Count), replyMarkup: markup, cancellationToken: ct);
                }
            }
            // 3. НАВИГАЦИЯ ПО ЛОКАЦИЯМ (/locations)
            else if (command == "/locations")
            {
                var rootLocations = await db.Set<StorageLocation>()
                    .Where(l => l.UserId == userId && l.ParentLocationId == null)
                    .ToListAsync(ct);

                var buttons = rootLocations.Select(l => new[] { InlineKeyboardButton.WithCallbackData($"📍 {l.Name}", $"loc_{l.Id}") });
                var markup = new InlineKeyboardMarkup(buttons);

                await botClient.SendMessage(chatId, localizer["TG_LOCATIONS_ROOT"].Value, parseMode: ParseMode.Markdown, replyMarkup: markup, cancellationToken: ct);
            }
            // 4. КОНКРЕТНАЯ ЛОКАЦИЯ ПО ИМЕНИ (/location <имя>)
            else if (command == "/location")
            {
                var term = string.Join(" ", parts[1..]).ToLower().Trim();
                var loc = await db.Set<StorageLocation>().FirstOrDefaultAsync(l => l.UserId == userId && l.Name.ToLower() == term, ct);

                if (loc == null)
                {
                    await botClient.SendMessage(chatId, localizer["TG_LOCATION_NOT_FOUND"].Value, replyMarkup: mainReplyKeyboard, cancellationToken: ct);
                    return;
                }

                await SendLocationInfoAsync(botClient, chatId, loc, db, localizer, ct);
            }
            // 5. НАПОМИНАНИЯ (/reminders или кнопка 🔔 Напоминания)
            else if (command == "/reminders" || messageText == localizer["TG_BTN_REMINDERS"].Value)
            {
                var myBorrowedItems = await db.Set<Item>().Include(i => i.Reminders)
                    .Where(i => i.UserId == userId && i.Status == ItemStatus.Borrowed).ToListAsync(ct);

                var myLentItems = await db.Set<Item>().Include(i => i.Reminders)
                    .Where(i => i.UserId == userId && i.Status == ItemStatus.Lent).ToListAsync(ct);

                var allItemIds = myBorrowedItems.Select(i => i.Id).Concat(myLentItems.Select(i => i.Id)).ToList();
                var activeLendings = await db.Set<Lending>()
                    .Where(l => allItemIds.Contains(l.ItemId) && l.UserId == userId && l.ReturnedDate == null)
                    .ToListAsync(ct);

                var sb = new System.Text.StringBuilder();
                sb.AppendLine(localizer["TG_REMINDERS_TITLE"].Value);
                sb.AppendLine();

                sb.AppendLine(localizer["TG_REMINDERS_BORROWED_HEADER"].Value);
                if (!myBorrowedItems.Any()) sb.AppendLine(localizer["TG_REMINDERS_NO_BORROWED"].Value + "\n");
                else
                {
                    foreach (var item in myBorrowedItems)
                    {
                        var lending = activeLendings.FirstOrDefault(l => l.ItemId == item.Id);
                        var activeReminder = item.Reminders.FirstOrDefault(r => !r.IsCompleted);
                        var dueDate = lending?.ExpectedReturnDate ?? activeReminder?.TriggerAt;
                        var dateStr = dueDate.HasValue ? dueDate.Value.ToString("dd.MM.yyyy") : localizer["TG_REMINDERS_NO_DATE"].Value;
                        sb.AppendLine($"• *{item.Name}* — {localizer["TG_REMINDERS_RETURN_BY"].Value}: {dateStr}");
                    }
                    sb.AppendLine();
                }

                sb.AppendLine(localizer["TG_REMINDERS_LENT_HEADER"].Value);
                if (!myLentItems.Any()) sb.AppendLine(localizer["TG_REMINDERS_NO_LENT"].Value);
                else
                {
                    foreach (var item in myLentItems)
                    {
                        var lending = activeLendings.FirstOrDefault(l => l.ItemId == item.Id);
                        var activeReminder = item.Reminders.FirstOrDefault(r => !r.IsCompleted);
                        var dueDate = lending?.ExpectedReturnDate ?? activeReminder?.TriggerAt;
                        var dateStr = dueDate.HasValue ? dueDate.Value.ToString("dd.MM.yyyy") : localizer["TG_REMINDERS_NO_DATE"].Value;
                        sb.AppendLine($"• *{item.Name}* — {localizer["TG_REMINDERS_RETURN_BY"].Value}: {dateStr}");
                    }
                }

                await botClient.SendMessage(chatId, sb.ToString(), parseMode: ParseMode.Markdown, replyMarkup: mainReplyKeyboard, cancellationToken: ct);
            }
            // 6. ДОБАВИТЬ (/add или кнопка ➕ Добавить вещь)
            else if (command == "/add" || messageText == localizer["TG_BTN_ADD_ITEM"].Value)
            {
                if (messageText == localizer["TG_BTN_ADD_ITEM"].Value || parts.Length < 2)
                {
                    await botClient.SendMessage(chatId, localizer["TG_ADD_USAGE"].Value, parseMode: ParseMode.Markdown, replyMarkup: mainReplyKeyboard, cancellationToken: ct);
                    return;
                }

                var itemName = string.Join(" ", parts[1..]).Trim();

                var defaultCategory = await db.Set<Category>().FirstOrDefaultAsync(c => c.UserId == userId, ct);
                if (defaultCategory == null)
                {
                    defaultCategory = new Category { Id = Guid.NewGuid(), UserId = userId, Name = localizer["TG_DEFAULT_CATEGORY_NAME"].Value };
                    db.Set<Category>().Add(defaultCategory);
                    await db.SaveChangesAsync(ct);
                }

                var defaultLocation = await db.Set<StorageLocation>().FirstOrDefaultAsync(l => l.UserId == userId, ct);
                if (defaultLocation == null)
                {
                    defaultLocation = new StorageLocation { Id = Guid.NewGuid(), UserId = userId, Name = "Other" };
                    db.Set<StorageLocation>().Add(defaultLocation);
                    await db.SaveChangesAsync(ct);
                }

                var dto = new CreateItemDto
                {
                    Name = itemName,
                    CategoryId = defaultCategory.Id,
                    StorageLocationId = defaultLocation.Id,
                    Status = ItemStatus.Active,
                    Details = new ItemFinanceDto { Currency = "USD" }
                };

                await mediator.Send(new CreateItemCommand(userId, dto, new List<IFormFile>()), ct);
                await botClient.SendMessage(chatId, string.Format(localizer["TG_ADD_SUCCESS"].Value, itemName), parseMode: ParseMode.Markdown, replyMarkup: mainReplyKeyboard, cancellationToken: ct);
            }
            else if (command == "/help")
            {
                await botClient.SendMessage(chatId, localizer["TG_HELP_TEXT"].Value, parseMode: ParseMode.Markdown, replyMarkup: mainReplyKeyboard, cancellationToken: ct);
            }
        }

        private ReplyKeyboardMarkup GetMainReplyKeyboard(IStringLocalizer<SharedResource> localizer)
        {
            return new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton[] { localizer["TG_BTN_FIND_ITEMS"].Value, localizer["TG_BTN_REMINDERS"].Value },
                new KeyboardButton[] { localizer["TG_BTN_ADD_ITEM"].Value }
            })
            { ResizeKeyboard = true };
        }

        private async Task HandleCallbackQueryAsync(ITelegramBotClient botClient, CallbackQuery callback, AppDbContext db, IStringLocalizer<SharedResource> localizer, CancellationToken ct)
        {
            var data = callback.Data;
            if (string.IsNullOrEmpty(data)) return;
            var chatId = callback.Message!.Chat.Id;

            if (data.StartsWith("where_"))
            {
                var itemId = Guid.Parse(data.Replace("where_", ""));
                var item = await db.Set<Item>().FindAsync(itemId);
                if (item != null)
                {
                    var tree = await BuildLocationTreeAsync(db, item.StorageLocationId, localizer, ct);
                    await botClient.SendMessage(chatId, $"📦 *{item.Name}*\n\n{tree}", parseMode: ParseMode.Markdown, cancellationToken: ct);
                    await botClient.AnswerCallbackQuery(callback.Id, cancellationToken: ct);
                }
            }
            else if (data.StartsWith("loc_"))
            {
                var locId = Guid.Parse(data.Replace("loc_", ""));
                var loc = await db.Set<StorageLocation>().FindAsync(locId);
                if (loc != null)
                {
                    await SendLocationInfoAsync(botClient, chatId, loc, db, localizer, ct);
                    await botClient.AnswerCallbackQuery(callback.Id, cancellationToken: ct);
                }
            }
            else if (data.StartsWith("sublocs_"))
            {
                var locId = Guid.Parse(data.Replace("sublocs_", ""));
                var subLocs = await db.Set<StorageLocation>().Where(l => l.ParentLocationId == locId).ToListAsync(ct);

                var buttons = subLocs.Select(l => new[] { InlineKeyboardButton.WithCallbackData($"📁 {l.Name}", $"loc_{l.Id}") });
                var markup = new InlineKeyboardMarkup(buttons);

                await botClient.SendMessage(chatId, localizer["TG_BTN_SUBLOCATIONS"].Value + ":", replyMarkup: markup, cancellationToken: ct);
                await botClient.AnswerCallbackQuery(callback.Id, cancellationToken: ct);
            }
            else if (data.StartsWith("locitems_"))
            {
                var locId = Guid.Parse(data.Replace("locitems_", ""));
                var items = await db.Set<Item>().Where(i => i.StorageLocationId == locId && i.Status == ItemStatus.Active).ToListAsync(ct);

                var sb = new System.Text.StringBuilder();
                sb.AppendLine($"📦 *{localizer["TG_BTN_ALL_ITEMS"].Value}:*");
                foreach (var item in items) sb.AppendLine($"• {item.Name}");

                await botClient.SendMessage(chatId, sb.ToString(), parseMode: ParseMode.Markdown, cancellationToken: ct);
                await botClient.AnswerCallbackQuery(callback.Id, cancellationToken: ct);
            }
            else if (data.StartsWith("move_"))
            {
                await botClient.AnswerCallbackQuery(callback.Id, localizer["TG_DEV_STUB"].Value, showAlert: true, cancellationToken: ct);
            }
        }

        private async Task SendLocationInfoAsync(ITelegramBotClient botClient, long chatId, StorageLocation loc, AppDbContext db, IStringLocalizer<SharedResource> localizer, CancellationToken ct)
        {
            var itemsCount = await db.Set<Item>().CountAsync(i => i.StorageLocationId == loc.Id && i.Status == ItemStatus.Active, ct);
            var subLocsCount = await db.Set<StorageLocation>().CountAsync(l => l.ParentLocationId == loc.Id, ct);

            var latestItems = await db.Set<Item>()
                .Where(i => i.StorageLocationId == loc.Id && i.Status == ItemStatus.Active)
                .OrderByDescending(i => i.CreatedAt)
                .Take(5)
                .ToListAsync(ct);

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"📍 *{loc.Name}*\n");
            sb.AppendLine(string.Format(localizer["TG_LOCATION_INFO"].Value, itemsCount, subLocsCount));

            if (latestItems.Any())
            {
                sb.AppendLine("\n" + localizer["TG_LOCATION_LATEST_ITEMS"].Value);
                foreach (var item in latestItems) sb.AppendLine($"• {item.Name}");
            }

            var buttons = new List<InlineKeyboardButton>();
            if (itemsCount > 0) buttons.Add(InlineKeyboardButton.WithCallbackData(localizer["TG_BTN_ALL_ITEMS"].Value, $"locitems_{loc.Id}"));
            if (subLocsCount > 0) buttons.Add(InlineKeyboardButton.WithCallbackData(localizer["TG_BTN_SUBLOCATIONS"].Value, $"sublocs_{loc.Id}"));

            var markup = buttons.Any() ? new InlineKeyboardMarkup(buttons) : null;
            await botClient.SendMessage(chatId, sb.ToString(), parseMode: ParseMode.Markdown, replyMarkup: markup, cancellationToken: ct);
        }

        private async Task<string> BuildLocationChainInlineAsync(AppDbContext db, Guid? locationId, IStringLocalizer<SharedResource> localizer, CancellationToken ct)
        {
            if (locationId == null) return localizer["TG_LOCATION_NOT_SET"].Value;
            var path = new List<string>();
            var currentId = locationId;

            while (currentId.HasValue)
            {
                var loc = await db.Set<StorageLocation>().FirstOrDefaultAsync(l => l.Id == currentId.Value, ct);
                if (loc == null) break;
                path.Insert(0, loc.Name);
                currentId = loc.ParentLocationId;
            }
            return path.Count > 0 ? string.Join(" ➔ ", path) : localizer["TG_LOCATION_NOT_SET"].Value;
        }

        private async Task<string> BuildLocationTreeAsync(AppDbContext db, Guid? locationId, IStringLocalizer<SharedResource> localizer, CancellationToken ct)
        {
            if (locationId == null) return localizer["TG_LOCATION_NOT_SET"].Value;
            var path = new List<string>();
            var currentId = locationId;

            while (currentId.HasValue)
            {
                var loc = await db.Set<StorageLocation>().FirstOrDefaultAsync(l => l.Id == currentId.Value, ct);
                if (loc == null) break;
                path.Insert(0, loc.Name);
                currentId = loc.ParentLocationId;
            }

            if (path.Count == 0) return localizer["TG_LOCATION_NOT_FOUND"].Value;

            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < path.Count; i++)
            {
                if (i == 0) sb.AppendLine($"📍 {path[i]}");
                else sb.AppendLine($"{new string(' ', (i - 1) * 4)}└── {path[i]}");
            }
            return sb.ToString().TrimEnd();
        }

        private async Task<Guid> GetUserIdByChatIdAsync(AppDbContext db, long chatId, CancellationToken ct)
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.TelegramChatId == chatId, ct);
            return user?.Id ?? Guid.Empty;
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