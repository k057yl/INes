using INest.Features.Telegram.Dtos;
using MediatR;

namespace INest.Features.Telegram.Commands.GenerateTelegramToken
{
    public record GenerateTelegramTokenCommand(Guid UserId) : IRequest<TelegramStatusDto>;
}
