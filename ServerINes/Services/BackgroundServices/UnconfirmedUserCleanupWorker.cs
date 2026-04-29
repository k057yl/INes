using INest.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace INest.Services.BackgroundServices
{
    public class UnconfirmedUserCleanupWorker : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<UnconfirmedUserCleanupWorker> _logger;

        public UnconfirmedUserCleanupWorker(IServiceProvider services, ILogger<UnconfirmedUserCleanupWorker> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _services.CreateScope();
                    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

                    var cutoffTime = DateTime.UtcNow.AddHours(-24);

                    var unconfirmedUsers = userManager.Users
                        .Where(u => !u.EmailConfirmed && u.CreatedAt < cutoffTime)
                        .ToList();

                    foreach (var user in unconfirmedUsers)
                    {
                        var result = await userManager.DeleteAsync(user);
                        if (result.Succeeded)
                        {
                            _logger.LogInformation("Deleted unconfirmed user: {Email}", user.Email);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while cleaning up unconfirmed users.");
                }

                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }
}