using System.Security.Claims;
using INest.Data.Entities.Infrastructure;
using INest.Infrastructure;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Shouldly;

namespace INest.Tests.Infrastructure.Identity
{
    public class TokenServiceTests
    {
        private readonly IConfiguration _configMock = Substitute.For<IConfiguration>();
        private readonly string _testSecret = "super_secret_key_that_is_at_least_32_bytes_long_12345";

        public TokenServiceTests()
        {
            _configMock["Jwt:Key"].Returns(_testSecret);
            _configMock["Jwt:Issuer"].Returns("INestIssuer");
            _configMock["Jwt:Audience"].Returns("INestAudience");
        }

        [Fact]
        public void GenerateJwtToken_ShouldCreateValidToken_WithUserClaims()
        {
            // Arrange
            var service = new TokenService(_configMock);
            var userId = Guid.NewGuid();
            var user = new AppUser { Id = userId, UserName = "test@inest.com", DisplayName = "Роман" };
            var roles = new List<string> { "User", "Admin" };

            // Act
            var token = service.GenerateJwtToken(user, roles);

            // Assert
            token.ShouldNotBeNullOrEmpty();

            // Проверяем возможность обратно достать Principal через GetPrincipalFromExpiredToken
            var principal = service.GetPrincipalFromExpiredToken(token);
            principal.FindFirstValue(ClaimTypes.NameIdentifier).ShouldBe(userId.ToString());
            principal.FindFirstValue(ClaimTypes.Name).ShouldBe("test@inest.com");
            principal.FindFirstValue(ClaimTypes.GivenName).ShouldBe("Роман");
            principal.FindAll(ClaimTypes.Role).ShouldContain(c => c.Value == "Admin");
        }

        [Fact]
        public void HashRefreshToken_ShouldBeDeterministic()
        {
            // Arrange
            var service = new TokenService(_configMock);
            var refreshToken = service.GenerateRefreshToken();

            // Act
            var hash1 = service.HashRefreshToken(refreshToken);
            var hash2 = service.HashRefreshToken(refreshToken);

            // Assert
            hash1.ShouldBe(hash2);
            hash1.ShouldNotBe(refreshToken);
        }
    }
}