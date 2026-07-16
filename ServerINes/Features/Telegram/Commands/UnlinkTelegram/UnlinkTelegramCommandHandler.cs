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
            var updatedRows = await _db.Users
                .Where(u => u.Id == request.UserId)
                .ExecuteUpdateAsync(s => s.SetProperty(u => u.TelegramChatId, (long?)null), ct);

            await _db.Set<TelegramConnectionCode>()
                .Where(c => c.UserId == request.UserId)
                .ExecuteDeleteAsync(ct);

            return updatedRows > 0;
        }
    }
}
