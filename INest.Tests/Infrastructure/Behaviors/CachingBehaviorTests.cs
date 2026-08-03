using INest.Infrastructure.Behaviors;
using INest.Infrastructure.Caching;
using INest.Infrastructure.Tracker;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;

namespace INest.Tests.Infrastructure.Behaviors
{
    public class CachingBehaviorTests
    {
        public class TestCacheableQuery : IRequest<string>, ICacheableQuery
        {
            public Guid UserId { get; set; } = Guid.NewGuid();
            public string CacheKey => $"test_key_{UserId}";
            public TimeSpan? Expiration => TimeSpan.FromMinutes(5);
        }

        [Fact]
        public async Task Handle_ShouldReturnFromCache_WhenKeyExists()
        {
            // Arrange
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var trackerMock = Substitute.For<ICacheTracker>();
            var loggerMock = Substitute.For<ILogger<CachingBehavior<TestCacheableQuery, string>>>();

            var query = new TestCacheableQuery();
            memoryCache.Set(query.CacheKey, "CachedResult");

            var behavior = new CachingBehavior<TestCacheableQuery, string>(memoryCache, trackerMock, loggerMock);

            bool nextCalled = false;
            RequestHandlerDelegate<string> next = (ct) =>
            {
                nextCalled = true;
                return Task.FromResult("FreshResult");
            };

            // Act
            var result = await behavior.Handle(query, next, CancellationToken.None);

            // Assert
            result.ShouldBe("CachedResult");
            nextCalled.ShouldBeFalse();
        }

        [Fact]
        public async Task Handle_ShouldExecuteNextAndCache_WhenKeyDoesNotExist()
        {
            // Arrange
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var trackerMock = Substitute.For<ICacheTracker>();
            var loggerMock = Substitute.For<ILogger<CachingBehavior<TestCacheableQuery, string>>>();

            var query = new TestCacheableQuery();
            var behavior = new CachingBehavior<TestCacheableQuery, string>(memoryCache, trackerMock, loggerMock);

            RequestHandlerDelegate<string> next = (ct) => Task.FromResult("FreshResult");

            // Act
            var result = await behavior.Handle(query, next, CancellationToken.None);

            // Assert
            result.ShouldBe("FreshResult");

            memoryCache.TryGetValue(query.CacheKey, out string? cachedValue).ShouldBeTrue();
            cachedValue.ShouldBe("FreshResult");
            trackerMock.Received(1).GetToken(query.UserId);
        }
    }
}