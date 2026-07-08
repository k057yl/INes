using INest.Constants;
using INest.Data.Entities.Infrastructure;
using INest.Services.Interfaces;
using MediatR;

namespace INest.Services.Features.Reminders.Queries.GetActiveReminders
{
    public record GetActiveRemindersQuery(Guid UserId) : IRequest<IEnumerable<Reminder>>, ICacheableQuery
    {
        public string CacheKey => CacheConstants.GET_ACTIVE_REMINDERS_KEY(UserId);
        public TimeSpan? Expiration => TimeSpan.FromMinutes(30);
    }
}
