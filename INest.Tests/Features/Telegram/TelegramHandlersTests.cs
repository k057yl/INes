using INest.Data.Entities.Core;
using INest.Data.Entities.Infrastructure;
using INest.Features.Telegram.Commands.GenerateTelegramToken;
using INest.Features.Telegram.Queries.GetTelegramStatus;
using INest.Features.Telegram.Queries.SearchItems;
using INest.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Shouldly;

namespace INest.Tests.Features.Telegram
{
    public class TelegramHandlersTests
    {
        private readonly IConfiguration _configurationMock = Substitute.For<IConfiguration>();

        public TelegramHandlersTests()
        {
            _configurationMock["Telegram:BotUsername"].Returns("INestTestBot");
        }

        #region GenerateTelegramTokenCommandHandler Tests

        [Fact]
        public async Task GenerateTelegramToken_ShouldCreateCode_AndReturnStatusDto()
        {
            // Arrange
            using var db = DbContextFactory.Create();
            var userId = Guid.NewGuid();

            var handler = new GenerateTelegramTokenCommandHandler(db, _configurationMock);
            var command = new GenerateTelegramTokenCommand(userId);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.IsLinked.ShouldBeFalse();
            result.BotUsername.ShouldBe("INestTestBot");
            result.VerificationToken.ShouldNotBeNullOrEmpty();

            var codeInDb = await db.Set<TelegramConnectionCode>().FirstOrDefaultAsync(c => c.UserId == userId);
            codeInDb.ShouldNotBeNull();
            codeInDb.Code.ShouldBe(result.VerificationToken);
            codeInDb.ExpiryTime.ShouldBeGreaterThan(DateTime.UtcNow);
        }

        #endregion

        #region GetTelegramStatusQueryHandler Tests

        [Fact]
        public async Task GetTelegramStatus_ShouldReturnIsLinkedTrue_WhenChatIdExists()
        {
            // Arrange
            using var db = DbContextFactory.Create();
            var userId = Guid.NewGuid();

            var user = new AppUser
            {
                Id = userId,
                Email = "tg@inest.com",
                UserName = "tg@inest.com",
                DisplayName = "Роман",
                TelegramChatId = 123456789
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();

            var handler = new GetTelegramStatusQueryHandler(db, _configurationMock);
            var query = new GetTelegramStatusQuery(userId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.IsLinked.ShouldBeTrue();
            result.TelegramChatId.ShouldBe(123456789);
        }

        [Fact]
        public async Task GetTelegramStatus_ShouldReturnToken_WhenUserNotLinkedButHasActiveCode()
        {
            // Arrange
            using var db = DbContextFactory.Create();
            var userId = Guid.NewGuid();

            var user = new AppUser
            {
                Id = userId,
                Email = "notlinked@inest.com",
                UserName = "notlinked@inest.com",
                DisplayName = "Роман",
                TelegramChatId = null
            };

            var code = new TelegramConnectionCode
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Code = "valid_token_123",
                ExpiryTime = DateTime.UtcNow.AddMinutes(10)
            };

            db.Users.Add(user);
            db.Set<TelegramConnectionCode>().Add(code);
            await db.SaveChangesAsync();

            var handler = new GetTelegramStatusQueryHandler(db, _configurationMock);
            var query = new GetTelegramStatusQuery(userId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsLinked.ShouldBeFalse();
            result.VerificationToken.ShouldBe("valid_token_123");
            result.BotUsername.ShouldBe("INestTestBot");
        }

        #endregion

        #region SearchItemsHandler Tests

        [Fact]
        public async Task SearchItems_ShouldReturnFilteredItems_ForTelegramChatOwner()
        {
            // Arrange
            using var db = DbContextFactory.Create();
            var userId = Guid.NewGuid();
            long chatId = 987654321;

            var user = new AppUser
            {
                Id = userId,
                Email = "botuser@inest.com",
                UserName = "botuser@inest.com",
                DisplayName = "Пользователь Бота",
                TelegramChatId = chatId
            };

            var category = new Category { Id = Guid.NewGuid(), Name = "Тест", Color = "#00B894", UserId = userId };
            var location = new StorageLocation { Id = Guid.NewGuid(), Name = "Кладовка", UserId = userId };

            var item1 = new Item { Id = Guid.NewGuid(), Name = "Перфоратор Bosch", UserId = userId, CategoryId = category.Id, StorageLocationId = location.Id };
            var item2 = new Item { Id = Guid.NewGuid(), Name = "Дрель Makita", UserId = userId, CategoryId = category.Id, StorageLocationId = location.Id };
            var soldItem = new Item { Id = Guid.NewGuid(), Name = "Проданный перфоратор", UserId = userId, CategoryId = category.Id, StorageLocationId = location.Id };
            soldItem.Sell();

            db.Users.Add(user);
            db.Categories.Add(category);
            db.StorageLocations.Add(location);
            db.Items.AddRange(item1, item2, soldItem);
            await db.SaveChangesAsync();

            var handler = new SearchItemsHandler(db);
            var query = new SearchItemsQuery(chatId, "перфоратор");

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Count.ShouldBe(1);
            result[0].Name.ShouldBe("Перфоратор Bosch");
            result[0].StorageLocationName.ShouldBe("Кладовка");
        }

        #endregion
    }
}