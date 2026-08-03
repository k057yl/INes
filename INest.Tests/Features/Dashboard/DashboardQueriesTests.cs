using INest.Data.Entities.Core;
using INest.Data.Entities.Finances;
using INest.Data.Entities.Infrastructure;
using INest.Features.Dashboard.DTOs;
using INest.Features.Dashboard.Queries.GetDashboardStats;
using INest.Features.Dashboard.Queries.GetGeneralStats;
using INest.Features.Dashboard.Queries.GetLendingsAndWarrantiesStats;
using INest.Features.Dashboard.Queries.GetRemindersStats;
using INest.Tests.Helpers;
using MediatR;
using NSubstitute;
using Shouldly;

namespace INest.Tests.Features.Dashboard
{
    public class DashboardQueriesTests
    {
        private readonly IMediator _mediatorMock = Substitute.For<IMediator>();

        #region GetGeneralStatsQueryHandler Tests

        [Fact]
        public async Task GetGeneralStats_ShouldCalculateCountsAccurately_IgnoringArchivedAndSoldForTotalItems()
        {
            // Arrange
            using var db = DbContextFactory.Create();
            var userId = Guid.NewGuid();
            var category = new Category { Id = Guid.NewGuid(), Name = "Тест", Color = "#00B894", UserId = userId };

            // Добавили Name = "..." для каждого предмета
            var activeItem = new Item { Id = Guid.NewGuid(), Name = "Активный", UserId = userId, CategoryId = category.Id };
            var lentItem = new Item { Id = Guid.NewGuid(), Name = "Выданный", UserId = userId, CategoryId = category.Id };
            lentItem.Lend();

            var soldItem = new Item { Id = Guid.NewGuid(), Name = "Проданный", UserId = userId, CategoryId = category.Id };
            soldItem.Sell();

            var archivedItem = new Item { Id = Guid.NewGuid(), Name = "В архиве", UserId = userId, CategoryId = category.Id };
            archivedItem.Archive();

            var location1 = new StorageLocation { Id = Guid.NewGuid(), UserId = userId, Name = "Шкаф" };
            var location2 = new StorageLocation { Id = Guid.NewGuid(), UserId = userId, Name = "Гараж" };

            var lending = new Lending
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ItemId = lentItem.Id,
                PersonName = "Олег",
                ReturnedDate = null
            };

            db.Categories.Add(category);
            db.Items.AddRange(activeItem, lentItem, soldItem, archivedItem);
            db.StorageLocations.AddRange(location1, location2);
            db.Lendings.Add(lending);
            await db.SaveChangesAsync();

            var handler = new GetGeneralStatsQueryHandler(db);
            var query = new GetGeneralStatsQuery(userId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.TotalItemsCount.ShouldBe(2);
            result.TotalLocationsCount.ShouldBe(2);
            result.LentItemsCount.ShouldBe(1);
            result.SoldItemsCount.ShouldBe(1);
        }

        #endregion

        #region GetRemindersStatsQueryHandler Tests

        [Fact]
        public async Task GetRemindersStats_ShouldAssignCorrectSeverity_AndExcludeCompleted()
        {
            // Arrange
            using var db = DbContextFactory.Create();
            var userId = Guid.NewGuid();
            var nowUtc = new DateTime(2026, 8, 1, 12, 0, 0, DateTimeKind.Utc);

            var category = new Category { Id = Guid.NewGuid(), Name = "Тест", Color = "#00B894", UserId = userId };
            var item = new Item { Id = Guid.NewGuid(), Name = "Пила", UserId = userId, CategoryId = category.Id };

            var expiredReminder = new Reminder
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ItemId = item.Id,
                Item = item,
                Title = "Просрочено",
                TriggerAt = nowUtc.AddDays(-1),
                IsCompleted = false
            };

            var warningReminder = new Reminder
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ItemId = item.Id,
                Item = item,
                Title = "Скоро",
                TriggerAt = nowUtc.AddDays(2),
                IsCompleted = false
            };

            var infoReminder = new Reminder
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ItemId = item.Id,
                Item = item,
                Title = "Далеко",
                TriggerAt = nowUtc.AddDays(10),
                IsCompleted = false
            };

            var completedReminder = new Reminder
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ItemId = item.Id,
                Item = item,
                Title = "Готово",
                TriggerAt = nowUtc.AddDays(-5),
                IsCompleted = true
            };

            db.Categories.Add(category);
            db.Items.Add(item);
            db.Reminders.AddRange(expiredReminder, warningReminder, infoReminder, completedReminder);
            await db.SaveChangesAsync();

            var handler = new GetRemindersStatsQueryHandler(db);
            var query = new GetRemindersStatsQuery(userId, nowUtc);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.ExpiredCount.ShouldBe(1);
            result.ActiveCount.ShouldBe(2);
            result.Items.Count.ShouldBe(3);

            result.Items.First(i => i.ItemId == item.Id && i.Date == expiredReminder.TriggerAt).Severity.ShouldBe("danger");
            result.Items.First(i => i.ItemId == item.Id && i.Date == warningReminder.TriggerAt).Severity.ShouldBe("warning");
            result.Items.First(i => i.ItemId == item.Id && i.Date == infoReminder.TriggerAt).Severity.ShouldBe("info");
        }

        #endregion

        #region GetLendingsAndWarrantiesStatsQueryHandler Tests

        [Fact]
        public async Task GetLendingsAndWarranties_ShouldFilterWithin30DaysThreshold()
        {
            // Arrange
            using var db = DbContextFactory.Create();
            var userId = Guid.NewGuid();
            var nowUtc = new DateTime(2026, 8, 1, 12, 0, 0, DateTimeKind.Utc);

            var category = new Category { Id = Guid.NewGuid(), Name = "Тест", Color = "#00B894", UserId = userId };
            var item = new Item
            {
                Id = Guid.NewGuid(),
                Name = "Телевизор",
                UserId = userId,
                CategoryId = category.Id,
                Details = new ItemDetails { WarrantyExpiration = nowUtc.AddDays(15) }
            };

            var lending = new Lending
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ItemId = item.Id,
                Item = item,
                PersonName = "Антон",
                ExpectedReturnDate = nowUtc.AddDays(-2)
            };

            db.Categories.Add(category);
            db.Items.Add(item);
            db.Lendings.Add(lending);
            await db.SaveChangesAsync();

            var handler = new GetLendingsAndWarrantiesStatsQueryHandler(db);
            var query = new GetLendingsAndWarrantiesStatsQuery(userId, nowUtc);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.ExpiredLendingsCount.ShouldBe(1);
            result.ExpiringWarrantiesCount.ShouldBe(1);
            result.Items.Count.ShouldBe(2);

            var dangerLending = result.Items.FirstOrDefault(i => i.TypeKey == "DASHBOARD_STATS.LENT");
            dangerLending.ShouldNotBeNull();
            dangerLending.Severity.ShouldBe("danger");
        }

        #endregion

        #region GetDashboardStatsQueryHandler Tests (Master Aggregator)

        [Fact]
        public async Task GetDashboardStats_ShouldMergeCollections_AndSortDangerItemsFirst()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var nowUtc = DateTime.UtcNow;

            var generalStats = new GeneralStatsDto
            {
                TotalItemsCount = 10,
                TotalLocationsCount = 3,
                LentItemsCount = 2,
                SoldItemsCount = 5
            };

            var warningItem = new AttentionItemDto
            {
                ItemName = "Предупреждение",
                Severity = "warning",
                Date = nowUtc.AddDays(1)
            };

            var dangerItemLater = new AttentionItemDto
            {
                ItemName = "Опасность позже",
                Severity = "danger",
                Date = nowUtc.AddDays(-1)
            };

            var dangerItemEarlier = new AttentionItemDto
            {
                ItemName = "Опасность раньше",
                Severity = "danger",
                Date = nowUtc.AddDays(-5)
            };

            var remindersStats = new RemindersStatsDto
            {
                ExpiredCount = 2,
                ActiveCount = 1,
                Items = new List<AttentionItemDto> { warningItem, dangerItemLater }
            };

            var lendingsStats = new LendingsAndWarrantiesStatsDto
            {
                ExpiredLendingsCount = 1,
                ExpiringLendingsCount = 0,
                ExpiringWarrantiesCount = 0,
                Items = new List<AttentionItemDto> { dangerItemEarlier }
            };

            _mediatorMock.Send(Arg.Any<GetGeneralStatsQuery>(), Arg.Any<CancellationToken>()).Returns(generalStats);
            _mediatorMock.Send(Arg.Any<GetRemindersStatsQuery>(), Arg.Any<CancellationToken>()).Returns(remindersStats);
            _mediatorMock.Send(Arg.Any<GetLendingsAndWarrantiesStatsQuery>(), Arg.Any<CancellationToken>()).Returns(lendingsStats);

            var handler = new GetDashboardStatsQueryHandler(_mediatorMock);
            var query = new GetDashboardStatsQuery(userId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.TotalItemsCount.ShouldBe(10);
            result.AttentionItems.Count.ShouldBe(3);

            // Теперь "danger" элементы первыми:
            result.AttentionItems[0].ItemName.ShouldBe("Опасность раньше");
            result.AttentionItems[1].ItemName.ShouldBe("Опасность позже");
            result.AttentionItems[2].ItemName.ShouldBe("Предупреждение");
        }

        #endregion
    }
}