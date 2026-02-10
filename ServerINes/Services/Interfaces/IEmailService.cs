namespace INest.Services.Interfaces
{
    public interface IEmailService
    {
        public Task<bool> SendEmailAsync(string toEmail, string subject, string htmlContent);
    }
}
