using INest.Exceptions;
using QRCoder;
using static INest.Constants.LocalizationConstants;

namespace INest.Infrastructure.QrCode
{
    public class QrCodeService : IQrCodeService
    {
        public byte[] GeneratePngCode(string payload)
        {
            if (string.IsNullOrWhiteSpace(payload))
                throw new AppException(SYSTEM.ERRORS.VALIDATION_FAILED, 400);

            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);

            var qrCode = new BitmapByteQRCode(qrCodeData);

            return qrCode.GetGraphic(20);
        }

        public string GenerateBase64QrCode(string payload)
        {
            var bytes = GeneratePngCode(payload);
            return $"data:image/png;base64,{Convert.ToBase64String(bytes)}";
        }
    }
}