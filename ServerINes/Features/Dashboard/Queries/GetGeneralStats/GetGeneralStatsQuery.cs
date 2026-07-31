using INest.Features.Dashboard.DTOs;
using MediatR;

namespace INest.Features.Dashboard.Queries.GetGeneralStats
{
    public record GetGeneralStatsQuery(Guid UserId) : IRequest<GeneralStatsDto>;
}
