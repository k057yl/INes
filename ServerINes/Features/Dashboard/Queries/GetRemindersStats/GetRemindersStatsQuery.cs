using INest.Features.Dashboard.DTOs;
using MediatR;

namespace INest.Features.Dashboard.Queries.GetRemindersStats
{
    public record GetRemindersStatsQuery(Guid UserId, DateTime NowUtc) : IRequest<RemindersStatsDto>;
}
