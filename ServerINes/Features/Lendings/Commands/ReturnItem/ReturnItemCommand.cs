using INest.Features.Lendings.DTOs;
using MediatR;

namespace INest.Features.Lendings.Commands.ReturnItem
{
    public record ReturnItemCommand(Guid UserId, Guid ItemId, ReturnItemDto Dto) : IRequest<bool>;
}
