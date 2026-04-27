namespace INest.Services.Interfaces
{
    public interface ICacheableQuery
    {
        Guid UserId { get; }
        string CacheKey { get; }
        TimeSpan? Expiration { get; }
    }
}
