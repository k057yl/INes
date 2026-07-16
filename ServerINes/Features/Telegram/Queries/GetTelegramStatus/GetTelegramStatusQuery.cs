using INest.Features.Telegram.Dtos;
using MediatR;

namespace INest.Features.Telegram.Queries.GetTelegramStatus
{
    public record GetTelegramStatusQuery(Guid UserId) : IRequest<TelegramStatusDto>;
}
