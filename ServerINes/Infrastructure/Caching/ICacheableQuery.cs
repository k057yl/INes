namespace INest.Infrastructure.Caching
{
    public interface ICacheableQuery
    {
        Guid UserId { get; }
        string CacheKey { get; }
        TimeSpan? Expiration { get; }
    }
}
