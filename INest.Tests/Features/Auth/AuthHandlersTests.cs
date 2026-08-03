using System.Security.Claims;
using INest.Constants;
using INest.Data.Entities.Infrastructure;
using INest.Exceptions;
using INest.Features.Auth.Commands.ConfirmRegister;
using INest.Features.Auth.Commands.Login;
using INest.Features.Auth.Commands.RefreshToken;
using INest.Features.Auth.Commands.Register;
using INest.Features.Auth.Queries.GetMe;
using INest.Infrastructure.Email;
using INest.Infrastructure.Identity;
using INest.Infrastructure.Sanitizer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;

namespace INest.Tests.Features.Auth
{
    public class AuthHandlersTests
    {
        private readonly UserManager<AppUser> _userManagerMock;
        private readonly ITokenService _tokenServiceMock = Substitute.For<ITokenService>();
        private readonly IEmailService _emailServiceMock = Substitute.For<IEmailService>();
        private readonly ISanitizerService _sanitizerMock = Substitute.For<ISanitizerService>();
        private readonly IStringLocalizer<SharedResource> _localizerMock = Substitute.For<IStringLocalizer<SharedResource>>();

        public AuthHandlersTests()
        {
            var store = Substitute.For<IUserStore<AppUser>>();
            _userManagerMock = Substitute.For<UserManager<AppUser>>(
                store,
                Substitute.For<IOptions<IdentityOptions>>(),
                Substitute.For<IPasswordHasher<AppUser>>(),
                Array.Empty<IUserValidator<AppUser>>(),
                Array.Empty<IPasswordValidator<AppUser>>(),
                Substitute.For<ILookupNormalizer>(),
                Substitute.For<IdentityErrorDescriber>(),
                Substitute.For<IServiceProvider>(),
                Substitute.For<Microsoft.Extensions.Logging.ILogger<UserManager<AppUser>>>());

            _sanitizerMock.StripAllHtml(Arg.Any<string>()).Returns(x => x.Arg<string>()?.Trim());
            _localizerMock[Arg.Any<string>()].Returns(x => new LocalizedString(x.Arg<string>(), x.Arg<string>()));
        }

        #region RegisterHandler Tests

        [Fact]
        public async Task Register_ShouldCreateUserAndSendEmailCode()
        {
            // Arrange
            var email = "newuser@inest.com";
            _userManagerMock.FindByEmailAsync(email).Returns((AppUser)null!);
            _userManagerMock.CreateAsync(Arg.Any<AppUser>(), Arg.Any<string>()).Returns(IdentityResult.Success);
            _userManagerMock.AddToRoleAsync(Arg.Any<AppUser>(), Arg.Any<string>()).Returns(IdentityResult.Success);
            _userManagerMock.UpdateAsync(Arg.Any<AppUser>()).Returns(IdentityResult.Success);

            var handler = new RegisterHandler(_userManagerMock, _emailServiceMock, _localizerMock, _sanitizerMock);
            var command = new RegisterCommand("Роман", email, "Password123!");

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            await _userManagerMock.Received(1).CreateAsync(
                Arg.Is<AppUser>(u => u.Email == email && u.DisplayName == "Роман"),
                "Password123!");

            await _emailServiceMock.Received(1).SendEmailAsync(email, Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task Register_ShouldThrowAppException_WhenEmailAlreadyConfirmed()
        {
            // Arrange
            var email = "existing@inest.com";
            var user = new AppUser { Email = email, EmailConfirmed = true };
            _userManagerMock.FindByEmailAsync(email).Returns(user);

            var handler = new RegisterHandler(_userManagerMock, _emailServiceMock, _localizerMock, _sanitizerMock);
            var command = new RegisterCommand("Роман", email, "Password123!");

            // Act & Assert
            await Should.ThrowAsync<AppException>(async () =>
            {
                await handler.Handle(command, CancellationToken.None);
            });
        }

        #endregion

        #region ConfirmRegisterHandler Tests

        [Fact]
        public async Task ConfirmRegister_ShouldConfirmEmail_AndReturnAuthTokens()
        {
            // Arrange
            var email = "USER@INEST.COM";
            var code = "123456";
            var user = new AppUser
            {
                Id = Guid.NewGuid(),
                Email = email,
                VerificationCode = code,
                VerificationCodeExpiryTime = DateTime.UtcNow.AddMinutes(5),
                EmailConfirmed = false
            };

            _userManagerMock.FindByEmailAsync(email).Returns(user);
            _userManagerMock.IsInRoleAsync(user, SharedConstants.DEFAULT_ROLE).Returns(false);
            _userManagerMock.AddToRoleAsync(user, SharedConstants.DEFAULT_ROLE).Returns(IdentityResult.Success);
            _userManagerMock.GetRolesAsync(user).Returns(new List<string> { SharedConstants.DEFAULT_ROLE });
            _userManagerMock.UpdateAsync(user).Returns(IdentityResult.Success);

            _tokenServiceMock.GenerateJwtToken(user, Arg.Any<IList<string>>()).Returns("jwt_access_token");
            _tokenServiceMock.GenerateRefreshToken().Returns("refresh_token");
            _tokenServiceMock.HashRefreshToken("refresh_token").Returns("hashed_refresh_token");

            var handler = new ConfirmRegisterHandler(_userManagerMock, _tokenServiceMock);
            var command = new ConfirmRegisterCommand(email, code);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Token.ShouldBe("jwt_access_token");
            result.RefreshToken.ShouldBe("refresh_token");

            user.EmailConfirmed.ShouldBeTrue();
            user.VerificationCode.ShouldBeNull();
            user.RefreshToken.ShouldBe("hashed_refresh_token");
        }

        [Fact]
        public async Task ConfirmRegister_ShouldThrowAppException_WhenCodeIsExpired()
        {
            // Arrange
            var email = "EXPIRED@INEST.COM";
            var user = new AppUser
            {
                Email = email,
                VerificationCode = "123456",
                VerificationCodeExpiryTime = DateTime.UtcNow.AddMinutes(-10) // Просрочен
            };

            _userManagerMock.FindByEmailAsync(email).Returns(user);

            var handler = new ConfirmRegisterHandler(_userManagerMock, _tokenServiceMock);
            var command = new ConfirmRegisterCommand(email, "123456");

            // Act & Assert
            await Should.ThrowAsync<AppException>(async () =>
            {
                await handler.Handle(command, CancellationToken.None);
            });
        }

        #endregion

        #region LoginHandler Tests

        [Fact]
        public async Task Login_ShouldThrowAppException_WhenEmailNotConfirmed()
        {
            // Arrange
            var email = "unconfirmed@inest.com";
            var user = new AppUser { Email = email, EmailConfirmed = false };

            _userManagerMock.FindByEmailAsync(email).Returns(user);
            _userManagerMock.CheckPasswordAsync(user, "Password123!").Returns(true);

            var handler = new LoginHandler(_userManagerMock, _tokenServiceMock);
            var command = new LoginCommand(email, "Password123!", "Europe/Kyiv");

            // Act & Assert
            var ex = await Should.ThrowAsync<AppException>(async () =>
            {
                await handler.Handle(command, CancellationToken.None);
            });

            ex.StatusCode.ShouldBe(401);
        }

        [Fact]
        public async Task Login_ShouldSucceed_AndReturnTokens()
        {
            // Arrange
            var email = "user@inest.com";
            var user = new AppUser { Id = Guid.NewGuid(), Email = email, EmailConfirmed = true, TimeZoneId = "Europe/Kyiv" };

            _userManagerMock.FindByEmailAsync(email).Returns(user);
            _userManagerMock.CheckPasswordAsync(user, "Password123!").Returns(true);
            _userManagerMock.GetRolesAsync(user).Returns(new List<string> { SharedConstants.DEFAULT_ROLE });
            _userManagerMock.UpdateAsync(user).Returns(IdentityResult.Success);

            _tokenServiceMock.GenerateJwtToken(user, Arg.Any<IList<string>>()).Returns("jwt_access_token");
            _tokenServiceMock.GenerateRefreshToken().Returns("refresh_token");

            var handler = new LoginHandler(_userManagerMock, _tokenServiceMock);
            var command = new LoginCommand(email, "Password123!", "Europe/Kyiv");

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Token.ShouldBe("jwt_access_token");
            result.RefreshToken.ShouldBe("refresh_token");
        }

        #endregion

        #region RefreshTokenHandler Tests

        [Fact]
        public async Task RefreshToken_ShouldThrowAppException_WhenTokenHashDoesNotMatch()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            }));

            _tokenServiceMock.GetPrincipalFromExpiredToken("expired_jwt").Returns(claimsPrincipal);
            _tokenServiceMock.HashRefreshToken("invalid_refresh_token").Returns("mismatched_hash");

            var user = new AppUser
            {
                Id = Guid.Parse(userId),
                RefreshToken = "actual_db_hash",
                RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(10)
            };

            _userManagerMock.FindByIdAsync(userId).Returns(user);

            var handler = new RefreshTokenHandler(_userManagerMock, _tokenServiceMock);
            var command = new RefreshTokenCommand("expired_jwt", "invalid_refresh_token");

            // Act & Assert
            await Should.ThrowAsync<AppException>(async () =>
            {
                await handler.Handle(command, CancellationToken.None);
            });
        }

        #endregion

        #region GetMeHandler Tests

        [Fact]
        public async Task GetMe_ShouldReturnUserProfileAndRoles()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new AppUser
            {
                Id = userId,
                Email = "roman@inest.com",
                DisplayName = "Роман Роман"
            };

            _userManagerMock.FindByIdAsync(userId.ToString()).Returns(user);
            _userManagerMock.GetRolesAsync(user).Returns(new List<string> { "User", "Admin" });

            var handler = new GetMeHandler(_userManagerMock);
            var query = new GetMeQuery(userId.ToString());

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Email.ShouldBe("roman@inest.com");
            result.DisplayName.ShouldBe("Роман Роман");
            result.Roles.Count.ShouldBe(2);
            result.Roles.ShouldContain("Admin");
        }

        #endregion
    }
}