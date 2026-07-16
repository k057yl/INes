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

            var updatedRows = await _db.Users
                .Where(u => u.Id == connectionCode.UserId)
                .ExecuteUpdateAsync(s => s.SetProperty(u => u.TelegramChatId, request.ChatId), ct);

            if (updatedRows > 0)
            {
                await _db.Set<TelegramConnectionCode>()
                    .Where(c => c.Id == connectionCode.Id)
                    .ExecuteDeleteAsync(ct);

                return true;
            }

            return false;
        }
    }
}
