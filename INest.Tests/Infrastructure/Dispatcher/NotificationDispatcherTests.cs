using INest.Data.Entities.Infrastructure;
using INest.Infrastructure.Dispatcher;
using INest.Infrastructure.Email;
using INest.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;

namespace INest.Tests.Infrastructure.Dispatcher
{
    public class NotificationDispatcherTests
    {
        private readonly IEmailService _emailServiceMock = Substitute.For<IEmailService>();
        private readonly IConfiguration _configMock = Substitute.For<IConfiguration>();
        private readonly ILogger<NotificationDispatcher> _loggerMock = Substitute.For<ILogger<NotificationDispatcher>>();

        public NotificationDispatcherTests()
        {
            _configMock["Telegram:BotToken"].Returns((string?)null);
        }

        [Fact]
        public async Task SendAsync_ShouldSaveNotificationToDb_AndSendEmail_WhenTelegramNotLinked()
        {
            // Arrange
            using var db = DbContextFactory.Create();
            var userId = Guid.NewGuid();

            var user = new AppUser
            {
                Id = userId,
                Email = "user@inest.com",
                UserName = "user@inest.com",
                DisplayName = "Роман",
                TelegramChatId = null // TG не привязан
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();

            var dispatcher = new NotificationDispatcher(db, _emailServiceMock, _configMock, _loggerMock);

            // Act
            await dispatcher.SendAsync(userId, "Напоминание о вернувшейся вещи", "SUBJECT_KEY", "BODY_KEY", CancellationToken.None);

            // Assert
            // 1. Уведомление создалось в базе
            var notificationInDb = await db.Notifications.FirstOrDefaultAsync(n => n.UserId == userId);
            notificationInDb.ShouldNotBeNull();
            notificationInDb.Message.ShouldBe("Напоминание о вернувшейся вещи");
            notificationInDb.IsRead.ShouldBeFalse();

            // 2. Был вызвана отправка резервного Email
            await _emailServiceMock.Received(1).SendEmailAsync("user@inest.com", "SUBJECT_KEY", "BODY_KEY");
        }

        [Fact]
        public async Task SendAsync_ShouldNotSendEmail_WhenUserNotFound()
        {
            // Arrange
            using var db = DbContextFactory.Create();
            var dispatcher = new NotificationDispatcher(db, _emailServiceMock, _configMock, _loggerMock);

            // Act
            await dispatcher.SendAsync(Guid.NewGuid(), "Сообщение", "SUBJ", "BODY", CancellationToken.None);

            // Assert
            await _emailServiceMock.DidNotReceive().SendEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }
    }
}