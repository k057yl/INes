using INest.Models.DTOs.Lending;
using INest.Models.Entities;
using MediatR;

namespace INest.Services.Features.Lendings.Commands.LendItem
{
    public record LendItemCommand(Guid UserId, LendItemDto Dto) : IRequest<Lending>;
}
