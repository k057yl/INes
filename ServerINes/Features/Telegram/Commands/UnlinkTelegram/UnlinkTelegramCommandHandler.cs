using INest.Data.Entities.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace INest.Features.Telegram.Commands.UnlinkTelegram
{
    public class UnlinkTelegramCommandHandler : IRequestHandler<UnlinkTelegramCommand, bool>
    {
        private readonly AppDbContext _db;

        public UnlinkTelegramCommandHandler(AppDbContext db)
        {
            _db = db;
        }

        public async Task<bool> Handle(UnlinkTelegramCommand request, CancellationToken ct)
        {
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Id == request.UserId, ct);

            if (user == null) return false;

            user.TelegramChatId = null;

            var oldCodes = await _db.Set<TelegramConnectionCode>()
                .Where(c => c.UserId == request.UserId)
                .ToListAsync(ct);

            if (oldCodes.Count > 0)
            {
                _db.Set<TelegramConnectionCode>().RemoveRange(oldCodes);
            }

            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}