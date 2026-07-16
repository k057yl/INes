using MediatR;

namespace INest.Features.Telegram.Commands.UnlinkTelegram
{
    public record UnlinkTelegramCommand(Guid UserId) : IRequest<bool>;
}
