using INest.Constants;
using INest.Data.Enums;
using INest.Exceptions;
using Shouldly;

namespace INest.Tests.Exceptions
{
    public class EnumExtensionsTests
    {
        [Theory]
        [InlineData(ItemStatus.Active, LocalizationConstants.STATUS.ACTIVE)]
        [InlineData(ItemStatus.Lent, LocalizationConstants.STATUS.LENT)]
        [InlineData(ItemStatus.Sold, LocalizationConstants.STATUS.SOLD)]
        [InlineData(ItemStatus.Archived, LocalizationConstants.STATUS.ARCHIVED)]
        public void GetLocalizationKey_ForItemStatus_ShouldReturnCorrectKey(ItemStatus status, string expectedKey)
        {
            // Act
            var key = status.GetLocalizationKey();

            // Assert
            key.ShouldBe(expectedKey);
        }

        [Theory]
        [InlineData(ItemHistoryType.Created, LocalizationConstants.HISTORY.CREATED)]
        [InlineData(ItemHistoryType.Moved, LocalizationConstants.HISTORY.MOVED)]
        [InlineData(ItemHistoryType.Lent, LocalizationConstants.HISTORY.LENT)]
        [InlineData(ItemHistoryType.Returned, LocalizationConstants.HISTORY.RETURNED)]
        [InlineData(ItemHistoryType.ValueUpdated, LocalizationConstants.HISTORY.VALUE_UPDATED)]
        public void GetLocalizationKey_ForItemHistoryType_ShouldReturnCorrectKey(ItemHistoryType type, string expectedKey)
        {
            // Act
            var key = type.GetLocalizationKey();

            // Assert
            key.ShouldBe(expectedKey);
        }

        [Theory]
        [InlineData(ReminderType.Custom, LocalizationConstants.REMINDERS.CUSTOM)]
        [InlineData(ReminderType.Warranty, LocalizationConstants.REMINDERS.WARRANTY)]
        [InlineData(ReminderType.Maintenance, LocalizationConstants.REMINDERS.MAINTENANCE)]
        [InlineData(ReminderType.Insurance, LocalizationConstants.REMINDERS.INSURANCE)]
        public void GetLocalizationKey_ForReminderType_ShouldReturnCorrectKey(ReminderType type, string expectedKey)
        {
            // Act
            var key = type.GetLocalizationKey();

            // Assert
            key.ShouldBe(expectedKey);
        }

        [Fact]
        public void GetLocalizationKey_ForInvalidItemStatus_ShouldReturnDefaultErrorKey()
        {
            // Arrange
            var invalidStatus = (ItemStatus)999;

            // Act
            var key = invalidStatus.GetLocalizationKey();

            // Assert
            key.ShouldBe(LocalizationConstants.SYSTEM.DEFAULT_ERROR);
        }
    }
}