using INest.Data.Entities.Infrastructure;
using INest.Features.Telegram.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace INest.Features.Telegram.Commands.GenerateTelegramToken
{
    public class GenerateTelegramTokenCommandHandler : IRequestHandler<GenerateTelegramTokenCommand, TelegramStatusDto>
    {
        private readonly AppDbContext _db;
        private readonly string _botUsername;

        public GenerateTelegramTokenCommandHandler(AppDbContext db, IConfiguration configuration)
        {
            _db = db;
            _botUsername = configuration["Telegram:BotUsername"] ?? "INestKarakatsiyaBot";
        }

        public async Task<TelegramStatusDto> Handle(GenerateTelegramTokenCommand request, CancellationToken ct)
        {
            await _db.Set<TelegramConnectionCode>()
                .Where(c => c.UserId == request.UserId)
                .ExecuteDeleteAsync(ct);

            var token = Guid.NewGuid().ToString("N");

            var connectionCode = new TelegramConnectionCode
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Code = token,
                ExpiryTime = DateTime.UtcNow.AddMinutes(15)
            };

            await _db.Set<TelegramConnectionCode>().AddAsync(connectionCode, ct);
            await _db.SaveChangesAsync(ct);

            return new TelegramStatusDto
            {
                IsLinked = false,
                BotUsername = _botUsername,
                VerificationToken = token
            };
        }
    }
}
