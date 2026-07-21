using INest.Features.Dashboard.DTOs;
using MediatR;

namespace INest.Features.Dashboard.Queries.GetDashboardStats
{
    public record GetDashboardStatsQuery(Guid UserId) : IRequest<DashboardStatsDto>;
}
