using INest.Features.Dashboard.DTOs;
using MediatR;

namespace INest.Features.Dashboard.Queries.GetLendingsAndWarrantiesStats
{
    public record GetLendingsAndWarrantiesStatsQuery(Guid UserId, DateTime NowUtc) : IRequest<LendingsAndWarrantiesStatsDto>;
}
