using INest.Data.Entities.Infrastructure;
using Microsoft.AspNetCore.Identity;

namespace INest.Infrastructure.BackgroundServices.Cleanup
{
    public class UnconfirmedUserCleanupService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<UnconfirmedUserCleanupService> _logger;

        public UnconfirmedUserCleanupService(IServiceScopeFactory scopeFactory, ILogger<UnconfirmedUserCleanupService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task CleanupAsync(CancellationToken cancellationToken = default)
        {
            using var scope = _scopeFactory.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

            var cutoffTime = DateTime.UtcNow.AddHours(-24);

            var unconfirmedUsers = userManager.Users
                .Where(u => !u.EmailConfirmed && u.CreatedAt < cutoffTime)
                .ToList();

            foreach (var user in unconfirmedUsers)
            {
                if (cancellationToken.IsCancellationRequested) break;

                var result = await userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Удален неподтвержденный пользователь: {Email}", user.Email);
                }
                else
                {
                    _logger.LogError("Ошибка удаления пользователя {Email}: {Errors}", user.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
    }
}
