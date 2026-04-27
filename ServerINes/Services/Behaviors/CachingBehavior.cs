using MediatR;
using Microsoft.Extensions.Caching.Memory;
using INest.Services.Interfaces;
using INest.Services.Tracker;

namespace INest.Services.Behaviors
{
    public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IMemoryCache _cache;
        private readonly ICacheTracker _tracker;
        private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;

        public CachingBehavior(IMemoryCache cache, ICacheTracker tracker, ILogger<CachingBehavior<TRequest, TResponse>> logger)
        {
            _cache = cache;
            _tracker = tracker;
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (request is not ICacheableQuery cacheableRequest)
            {
                return await next();
            }

            var cacheKey = cacheableRequest.CacheKey;

            if (_cache.TryGetValue(cacheKey, out TResponse? cachedResponse))
            {
                _logger.LogInformation("Кэш сработал для ключа: {CacheKey}", cacheKey);
                return cachedResponse!;
            }

            var response = await next();

            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = cacheableRequest.Expiration ?? TimeSpan.FromHours(1)
            };

            cacheOptions.AddExpirationToken(_tracker.GetToken(cacheableRequest.UserId));

            _cache.Set(cacheKey, response, cacheOptions);

            return response;
        }
    }
}