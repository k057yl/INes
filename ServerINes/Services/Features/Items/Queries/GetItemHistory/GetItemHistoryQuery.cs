using INest.Models.Entities;
using MediatR;

namespace INest.Services.Features.Items.Queries.GetItemHistory
{
    public record GetItemHistoryQuery(Guid UserId, Guid ItemId) : IRequest<IEnumerable<ItemHistory>>;
}
