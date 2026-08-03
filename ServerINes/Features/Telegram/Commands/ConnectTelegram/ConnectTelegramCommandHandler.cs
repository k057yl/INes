using INest.Data.Entities.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace INest.Features.Telegram.Commands.ConnectTelegram
{
    public class ConnectTelegramCommandHandler : IRequestHandler<ConnectTelegramCommand, bool>
    {
        private readonly AppDbContext _db;

        public ConnectTelegramCommandHandler(AppDbContext db)
        {
            _db = db;
        }

        public async Task<bool> Handle(ConnectTelegramCommand request, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.Token)) return false;

            var connectionCode = await _db.Set<TelegramConnectionCode>()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Code == request.Token && c.ExpiryTime > DateTime.UtcNow, ct);

            if (connectionCode == null) return false;

            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Id == connectionCode.UserId, ct);

            if (user != null)
            {
                user.TelegramChatId = request.ChatId;
                _db.Set<TelegramConnectionCode>().Remove(connectionCode);

                await _db.SaveChangesAsync(ct);
                return true;
            }

            return false;
        }
    }
}