using INest.Constants;
using INest.Models.Enums;

namespace INest.Exceptions
{
    public static class EnumExtensions
    {
        public static string GetLocalizationKey(this ItemStatus status) => status switch
        {
            ItemStatus.Active => LocalizationConstants.STATUS.ACTIVE,
            ItemStatus.Lent => LocalizationConstants.STATUS.LENT,
            ItemStatus.Lost => LocalizationConstants.STATUS.LOST,
            ItemStatus.Broken => LocalizationConstants.STATUS.BROKEN,
            ItemStatus.Sold => LocalizationConstants.STATUS.SOLD,
            ItemStatus.Gifted => LocalizationConstants.STATUS.GIFTED,
            ItemStatus.Listed => LocalizationConstants.STATUS.LISTED,
            ItemStatus.Borrowed => LocalizationConstants.STATUS.BORROWED,
            _ => LocalizationConstants.SYSTEM.DEFAULT_ERROR
        };

        public static string GetLocalizationKey(this ItemHistoryType type) => type switch
        {
            ItemHistoryType.Created => LocalizationConstants.HISTORY.CREATED,
            ItemHistoryType.Moved => LocalizationConstants.HISTORY.MOVED,
            ItemHistoryType.StatusChanged => LocalizationConstants.HISTORY.STATUS_CHANGED,
            ItemHistoryType.Repaired => LocalizationConstants.HISTORY.REPAIRED,
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

        public static string GetLocalizationKey(this LendingDirection direction) => direction switch
        {
            LendingDirection.Out => LocalizationConstants.STATUS.LENT,
            LendingDirection.In => LocalizationConstants.STATUS.BORROWED,
            _ => LocalizationConstants.SYSTEM.DEFAULT_ERROR
        };
    }
}