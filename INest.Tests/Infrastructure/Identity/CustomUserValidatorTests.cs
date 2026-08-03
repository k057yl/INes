using INest.Data.Entities.Infrastructure;
using INest.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;

namespace INest.Tests.Infrastructure.Identity
{
    public class CustomUserValidatorTests
    {
        [Fact]
        public async Task ValidateAsync_ShouldIgnoreDuplicateUserNameError()
        {
            // Arrange
            var userValidator = new CustomUserValidator<AppUser>();

            var options = Options.Create(new IdentityOptions
            {
                User = new UserOptions
                {
                    RequireUniqueEmail = false,
                    AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+"
                }
            });

            var userManagerMock = Substitute.For<UserManager<AppUser>>(
                Substitute.For<IUserStore<AppUser>>(),
                options,
                Substitute.For<IPasswordHasher<AppUser>>(),
                Array.Empty<IUserValidator<AppUser>>(),
                Array.Empty<IPasswordValidator<AppUser>>(),
                Substitute.For<ILookupNormalizer>(),
                new IdentityErrorDescriber(),
                Substitute.For<IServiceProvider>(),
                Substitute.For<ILogger<UserManager<AppUser>>>());

            var userId = Guid.NewGuid();
            var existingUserId = Guid.NewGuid();

            var user = new AppUser { Id = userId, UserName = "DuplicateName", Email = "unique@inest.com" };
            var existingUser = new AppUser { Id = existingUserId, UserName = "DuplicateName", Email = "other@inest.com" };

            // Обязательно мокаем GetUserNameAsync, иначе Identity считает, что имени нет!
            userManagerMock.GetUserNameAsync(user).Returns("DuplicateName");
            userManagerMock.GetUserIdAsync(user).Returns(userId.ToString());

            userManagerMock.GetUserNameAsync(existingUser).Returns("DuplicateName");
            userManagerMock.GetUserIdAsync(existingUser).Returns(existingUserId.ToString());

            userManagerMock.FindByNameAsync("DuplicateName").Returns(existingUser);

            // Act
            var result = await userValidator.ValidateAsync(userManagerMock, user);

            // Assert - если что-то упадет, мы увидим код ошибки в падении
            var errorsList = result.Errors.Select(e => $"{e.Code}: {e.Description}").ToList();
            errorsList.ShouldBeEmpty();
            result.Succeeded.ShouldBeTrue();
        }
    }
}