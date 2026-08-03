using INest.Exceptions;
using INest.Infrastructure.QrCode;
using Shouldly;

namespace INest.Tests.Infrastructure.QrCode
{
    public class QrCodeServiceTests
    {
        [Fact]
        public void GeneratePngCode_ShouldReturnNonEmptyByteArray_WhenPayloadIsValid()
        {
            // Arrange
            var service = new QrCodeService();
            var payload = "https://inest.app/items/12345";

            // Act
            var result = service.GeneratePngCode(payload);

            // Assert
            result.ShouldNotBeNull();
            result.Length.ShouldBeGreaterThan(0);
        }

        [Fact]
        public void GeneratePngCode_ShouldThrowAppException_WhenPayloadIsEmpty()
        {
            // Arrange
            var service = new QrCodeService();

            // Act & Assert
            Should.Throw<AppException>(() => service.GeneratePngCode("   "));
        }
    }
}