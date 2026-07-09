using INest.Constants;
using INest.Data.Entities.Infrastructure;
using INest.Infrastructure.Caching;
using MediatR;

namespace INest.Features.Reminders.Queries.GetItemReminders
{
    public record GetItemRemindersQuery(Guid UserId, Guid ItemId) : IRequest<IEnumerable<Reminder>>, ICacheableQuery
    {
        public string CacheKey => CacheConstants.GET_ITEM_REMINDERS_KEY(UserId, ItemId);
        public TimeSpan? Expiration => TimeSpan.FromMinutes(30);
    }
}
