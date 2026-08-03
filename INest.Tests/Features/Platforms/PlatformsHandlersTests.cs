using INest.Data.Entities.Finances;
using INest.Exceptions;
using INest.Features.Platforms.Commands.CreatePlatform;
using INest.Features.Platforms.Commands.DeletePlatform;
using INest.Features.Platforms.Commands.UpdatePlatform;
using INest.Features.Platforms.DTOs;
using INest.Features.Platforms.Queries.GetPlatforms;
using INest.Infrastructure.Sanitizer;
using INest.Infrastructure.Tracker;
using INest.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Shouldly;

namespace INest.Tests.Features.Platforms
{
    public class PlatformsHandlersTests
    {
        private readonly ISanitizerService _sanitizerMock = Substitute.For<ISanitizerService>();
        private readonly ICacheTracker _trackerMock = Substitute.For<ICacheTracker>();

        public PlatformsHandlersTests()
        {
            _sanitizerMock.StripAllHtml(Arg.Any<string>()).Returns(x => x.Arg<string>()?.Trim());
        }

        #region CreatePlatformHandler Tests

        [Fact]
        public async Task CreatePlatform_ShouldCreatePlatform_AndInvalidateCache()
        {
            // Arrange
            using var db = DbContextFactory.Create();
            var userId = Guid.NewGuid();

            var dto = new PlatformDto { Name = "OLX" };
            var handler = new CreatePlatformHandler(db, _sanitizerMock, _trackerMock);
            var command = new CreatePlatformCommand(userId, dto);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Name.ShouldBe("OLX");
            result.UserId.ShouldBe(userId);

            var inDb = await db.Platforms.FirstOrDefaultAsync(p => p.Id == result.Id);
            inDb.ShouldNotBeNull();

            _trackerMock.Received(1).InvalidateUserCache(userId);
        }

        [Fact]
        public async Task CreatePlatform_ShouldThrowAppException_WhenNameIsInvalid()
        {
            // Arrange
            using var db = DbContextFactory.Create();
            _sanitizerMock.StripAllHtml(Arg.Any<string>()).Returns("");

            var dto = new PlatformDto { Name = "<script></script>" };
            var handler = new CreatePlatformHandler(db, _sanitizerMock, _trackerMock);

            // Act & Assert
            await Should.ThrowAsync<AppException>(async () =>
            {
                await handler.Handle(new CreatePlatformCommand(Guid.NewGuid(), dto), CancellationToken.None);
            });
        }

        #endregion

        #region UpdatePlatformHandler Tests

        [Fact]
        public async Task UpdatePlatform_ShouldUpdateNameSuccessfully()
        {
            // Arrange
            using var db = DbContextFactory.Create();
            var userId = Guid.NewGuid();

            var platform = new Platform { Id = Guid.NewGuid(), Name = "eBay", UserId = userId };
            db.Platforms.Add(platform);
            await db.SaveChangesAsync();

            var dto = new PlatformDto { Name = "eBay Kleinanzeigen" };
            var handler = new UpdatePlatformHandler(db, _sanitizerMock, _trackerMock);
            var command = new UpdatePlatformCommand(userId, platform.Id, dto);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Name.ShouldBe("eBay Kleinanzeigen");

            var inDb = await db.Platforms.FirstAsync(p => p.Id == platform.Id);
            inDb.Name.ShouldBe("eBay Kleinanzeigen");

            _trackerMock.Received(1).InvalidateUserCache(userId);
        }

        #endregion

        #region DeletePlatformHandler Tests

        [Fact]
        public async Task DeletePlatform_ShouldRemoveFromDb()
        {
            // Arrange
            using var db = DbContextFactory.Create();
            var userId = Guid.NewGuid();

            var platform = new Platform { Id = Guid.NewGuid(), Name = "Prom.ua", UserId = userId };
            db.Platforms.Add(platform);
            await db.SaveChangesAsync();

            var handler = new DeletePlatformHandler(db, _trackerMock);
            var command = new DeletePlatformCommand(userId, platform.Id);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldBeTrue();

            (await db.Platforms.FirstOrDefaultAsync(p => p.Id == platform.Id)).ShouldBeNull();
            _trackerMock.Received(1).InvalidateUserCache(userId);
        }

        #endregion

        #region GetPlatformsHandler Tests

        [Fact]
        public async Task GetPlatforms_ShouldReturnSortedByName()
        {
            // Arrange
            using var db = DbContextFactory.Create();
            var userId = Guid.NewGuid();

            var p1 = new Platform { Id = Guid.NewGuid(), Name = "Z-Store", UserId = userId };
            var p2 = new Platform { Id = Guid.NewGuid(), Name = "A-Store", UserId = userId };

            db.Platforms.AddRange(p1, p2);
            await db.SaveChangesAsync();

            var handler = new GetPlatformsHandler(db);
            var query = new GetPlatformsQuery(userId);

            // Act
            var result = (await handler.Handle(query, CancellationToken.None)).ToList();

            // Assert
            result.Count.ShouldBe(2);
            result[0].Name.ShouldBe("A-Store");
            result[1].Name.ShouldBe("Z-Store");
        }

        #endregion
    }
}