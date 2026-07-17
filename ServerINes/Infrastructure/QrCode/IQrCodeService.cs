namespace INest.Infrastructure.QrCode
{
    public interface IQrCodeService
    {
        byte[] GeneratePngCode(string payload);
    }
}
