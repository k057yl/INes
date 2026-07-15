using INest.Constants;
using INest.Data.Enums;

namespace INest.Exceptions
{
    public static class EnumExtensions
    {
        public static string GetLocalizationKey(this ItemStatus status) => status switch
        {
            ItemStatus.Active => LocalizationConstants.STATUS.ACTIVE,
            ItemStatus.Lent => LocalizationConstants.STATUS.LENT,
            ItemStatus.Sold => LocalizationConstants.STATUS.SOLD,
            ItemStatus.Archived => LocalizationConstants.STATUS.ARCHIVED,
            _ => LocalizationConstants.SYSTEM.DEFAULT_ERROR
        };

        public static string GetLocalizationKey(this ItemHistoryType type) => type switch
        {
            ItemHistoryType.Created => LocalizationConstants.HISTORY.CREATED,
            ItemHistoryType.Moved => LocalizationConstants.HISTORY.MOVED,
            ItemHistoryType.Lent => LocalizationConstants.HISTORY.LENT,
            ItemHistoryType.Returned => LocalizationConstants.HISTORY.RETURNED,
            ItemHistoryType.ValueUpdated => LocalizationConstants.HISTORY.VALUE_UPDATED,
            ItemHistoryType.ReminderCompleted => LocalizationConstants.HISTORY.REMINDER.COMPLETED,
            ItemHistoryType.ReminderScheduled => LocalizationConstants.HISTORY.REMINDER.SCHEDULED,
            _ => LocalizationConstants.SYSTEM.DEFAULT_ERROR
        };

        public static string GetLocalizationKey(this ReminderType type) => type switch
        {
            ReminderType.Custom => LocalizationConstants.REMINDERS.CUSTOM,
            ReminderType.Warranty => LocalizationConstants.REMINDERS.WARRANTY,
            ReminderType.Maintenance => LocalizationConstants.REMINDERS.MAINTENANCE,
            ReminderType.ReturnItem => LocalizationConstants.REMINDERS.RETURN_ITEM,
            ReminderType.Insurance => LocalizationConstants.REMINDERS.INSURANCE,
            ReminderType.Medical => LocalizationConstants.REMINDERS.MEDICAL,
            ReminderType.Tax => LocalizationConstants.REMINDERS.TAX,
            ReminderType.Subscription => LocalizationConstants.REMINDERS.SUBSCRIPTION,
            _ => LocalizationConstants.REMINDERS.CUSTOM
        };
    }
}