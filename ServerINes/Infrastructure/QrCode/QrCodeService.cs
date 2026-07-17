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
            using var qrCode = new PngByteQRCode(qrCodeData);

            return qrCode.GetGraphic(20);
        }
    }
}
