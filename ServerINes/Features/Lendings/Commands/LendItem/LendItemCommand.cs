using INest.Data.Entities.Finances;
using INest.Features.Lendings.DTOs;
using MediatR;

namespace INest.Features.Lendings.Commands.LendItem
{
    public record LendItemCommand(Guid UserId, LendItemDto Dto) : IRequest<Lending>;
}
