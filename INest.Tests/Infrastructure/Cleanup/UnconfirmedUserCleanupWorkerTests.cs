using INest.Data.Entities.Infrastructure;
using INest.Infrastructure.BackgroundServices.Cleanup;
using INest.Tests.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;

namespace INest.Tests.Infrastructure.BackgroundServices
{
    public class UnconfirmedUserCleanupServiceTests
    {
        [Fact]
        public async Task CleanupAsync_ShouldDeleteOnlyUnconfirmedUsersOlderThan24Hours()
        {
            // Arrange
            using var db = DbContextFactory.Create();
            var now = DateTime.UtcNow;

            var oldUnconfirmedUser = new AppUser { Id = Guid.NewGuid(), UserName = "old@inest.com", Email = "old@inest.com", DisplayName = "Old User", EmailConfirmed = false, CreatedAt = now.AddHours(-25) };
            var newUnconfirmedUser = new AppUser { Id = Guid.NewGuid(), UserName = "new@inest.com", Email = "new@inest.com", DisplayName = "New User", EmailConfirmed = false, CreatedAt = now.AddHours(-2) };
            var confirmedUser = new AppUser { Id = Guid.NewGuid(), UserName = "conf@inest.com", Email = "conf@inest.com", DisplayName = "Conf User", EmailConfirmed = true, CreatedAt = now.AddHours(-30) };

            db.Users.AddRange(oldUnconfirmedUser, newUnconfirmedUser, confirmedUser);
            await db.SaveChangesAsync();

            var userStore = new UserStore<AppUser, IdentityRole<Guid>, AppDbContext, Guid>(db);
            var userManager = new UserManager<AppUser>(
                userStore,
                Options.Create(new IdentityOptions()),
                new PasswordHasher<AppUser>(),
                Array.Empty<IUserValidator<AppUser>>(),
                Array.Empty<IPasswordValidator<AppUser>>(),
                new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(),
                Substitute.For<IServiceProvider>(),
                Substitute.For<ILogger<UserManager<AppUser>>>());

            var serviceScopeMock = Substitute.For<IServiceScope>();
            var serviceScopeFactoryMock = Substitute.For<IServiceScopeFactory>();

            serviceScopeFactoryMock.CreateScope().Returns(serviceScopeMock);

            var serviceProviderMock = Substitute.For<IServiceProvider>();
            serviceScopeMock.ServiceProvider.Returns(serviceProviderMock);
            serviceProviderMock.GetService(typeof(UserManager<AppUser>)).Returns(userManager);

            var loggerMock = Substitute.For<ILogger<UnconfirmedUserCleanupService>>();
            var cleanupService = new UnconfirmedUserCleanupService(serviceScopeFactoryMock, loggerMock);

            // Act
            await cleanupService.CleanupAsync(CancellationToken.None);

            // Assert
            (await db.Users.FindAsync(oldUnconfirmedUser.Id)).ShouldBeNull();
            (await db.Users.FindAsync(newUnconfirmedUser.Id)).ShouldNotBeNull();
            (await db.Users.FindAsync(confirmedUser.Id)).ShouldNotBeNull();
        }
    }
}