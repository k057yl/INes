using INest.Models.Entities;
using MediatR;

namespace INest.Services.Features.Items.Queries.GetItemById
{
    public record GetItemByIdQuery(Guid UserId, Guid ItemId) : IRequest<Item?>;
}