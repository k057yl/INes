using INest.Data.Entities.Core;
using INest.Data.Entities.Finances;
using INest.Data.Entities.Infrastructure;
using INest.Data.Enums;
using INest.Infrastructure.Email;
using Microsoft.EntityFrameworkCore;

public class LendingService
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<LendingService> _logger;

    public LendingService(AppDbContext context, IEmailService emailService, ILogger<LendingService> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task LendAsync(
        Item item,
        string personName,
        string? contactEmail,
        DateTime? expectedReturnDate,
        bool sendNotification)
    {
        item.Lend();

        var lending = new Lending
        {
            Id = Guid.NewGuid(),
            ItemId = item.Id,
            UserId = item.UserId,
            DateGiven = DateTime.UtcNow,
            ValueAtLending = item.EstimatedValue,
            PersonName = personName,
            ContactEmail = contactEmail,
            ExpectedReturnDate = expectedReturnDate,
            SendNotification = sendNotification,
            NotificationSent = false
        };

        _context.Lendings.Add(lending);

        _context.ItemHistories.Add(new ItemHistory
        {
            Id = Guid.NewGuid(),
            ItemId = item.Id,
            UserId = item.UserId,
            Type = ItemHistoryType.Lent,
            NewValue = personName
        });

        if (sendNotification && expectedReturnDate.HasValue)
        {
            await CreateOrUpdateReminderAsync(item, expectedReturnDate.Value);
        }

        if (sendNotification && !string.IsNullOrWhiteSpace(contactEmail))
        {
            await TrySendEmailAsync(lending, item.Name, personName, expectedReturnDate);
        }
    }

    public async Task ReturnAsync(Item item)
    {
        item.Return();

        var lending = await _context.Lendings.FirstOrDefaultAsync(l => l.ItemId == item.Id);
        if (lending != null)
        {
            _context.Lendings.Remove(lending);
        }

        var reminder = await _context.Reminders
            .FirstOrDefaultAsync(r => r.ItemId == item.Id && r.Type == ReminderType.ReturnItem && !r.IsCompleted);
        if (reminder != null)
        {
            _context.Reminders.Remove(reminder);
        }

        _context.ItemHistories.Add(new ItemHistory
        {
            Id = Guid.NewGuid(),
            ItemId = item.Id,
            UserId = item.UserId,
            Type = ItemHistoryType.Returned
        });
    }

    private async Task CreateOrUpdateReminderAsync(Item item, DateTime expectedReturnDate)
    {
        var reminderDate = expectedReturnDate.AddDays(-1);
        if (reminderDate <= DateTime.UtcNow) return;

        var reminder = await _context.Reminders
            .FirstOrDefaultAsync(r => r.ItemId == item.Id && r.Type == ReminderType.ReturnItem && !r.IsCompleted);

        if (reminder != null)
        {
            reminder.TriggerAt = reminderDate;
        }
        else
        {
            _context.Reminders.Add(new Reminder
            {
                Id = Guid.NewGuid(),
                ItemId = item.Id,
                UserId = item.UserId,
                TriggerAt = reminderDate,
                Type = ReminderType.ReturnItem,
                IsCompleted = false
            });
        }
    }

    private async Task TrySendEmailAsync(Lending lending, string itemName, string personName, DateTime? expectedReturnDate)
    {
        try
        {
            await _emailService.SendLendingNotificationAsync(
                lending.ContactEmail!,
                itemName,
                personName,
                expectedReturnDate,
                false);

            lending.NotificationSent = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send lending notification for item {ItemId}", lending.ItemId);
        }
    }
}