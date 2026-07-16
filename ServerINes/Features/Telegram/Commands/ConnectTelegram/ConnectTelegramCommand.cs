using MediatR;

namespace INest.Features.Telegram.Commands.ConnectTelegram
{
    public record ConnectTelegramCommand(long ChatId, string Token) : IRequest<bool>;
}
