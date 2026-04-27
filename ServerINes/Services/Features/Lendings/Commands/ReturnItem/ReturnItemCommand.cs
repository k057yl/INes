using INest.Models.DTOs.Lending;
using MediatR;

namespace INest.Services.Features.Lendings.Commands.ReturnItem
{
    public record ReturnItemCommand(Guid UserId, Guid ItemId, ReturnItemDto Dto) : IRequest<bool>;
}
