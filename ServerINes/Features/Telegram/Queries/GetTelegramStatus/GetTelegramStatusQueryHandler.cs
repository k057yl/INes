using INest.Data.Entities.Infrastructure;
using INest.Features.Telegram.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace INest.Features.Telegram.Queries.GetTelegramStatus
{
    public class GetTelegramStatusQueryHandler : IRequestHandler<GetTelegramStatusQuery, TelegramStatusDto>
    {
        private readonly AppDbContext _db;
        private readonly string _botUsername;

        public GetTelegramStatusQueryHandler(AppDbContext db, IConfiguration configuration)
        {
            _db = db;
            _botUsername = configuration["Telegram:BotUsername"]
                             ?? configuration["TelegramBotSettings:BotUsername"]
                             ?? "INestHomeBot";
        }

        public async Task<TelegramStatusDto> Handle(GetTelegramStatusQuery request, CancellationToken ct)
        {
            var user = await _db.Users
                .AsNoTracking()
                .Select(u => new { u.Id, u.TelegramChatId })
                .FirstOrDefaultAsync(u => u.Id == request.UserId, ct);

            if (user == null) return new TelegramStatusDto { IsLinked = false, BotUsername = _botUsername };

            var isLinked = user.TelegramChatId.HasValue && user.TelegramChatId.Value != 0;

            var activeCode = isLinked ? null : await _db.Set<TelegramConnectionCode>()
                .AsNoTracking()
                .Where(c => c.UserId == request.UserId && c.ExpiryTime > DateTime.UtcNow)
                .Select(c => c.Code)
                .FirstOrDefaultAsync(ct);

            return new TelegramStatusDto
            {
                IsLinked = isLinked,
                TelegramChatId = user.TelegramChatId ?? 0,
                BotUsername = _botUsername,
                VerificationToken = activeCode
            };
        }
    }
}