using brevo_csharp.Api;
using brevo_csharp.Model;
using INest.Services.Interfaces;
using Configuration = brevo_csharp.Client.Configuration;

namespace INest.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly string _apiKey;
        private readonly string _fromEmail;
        private readonly string _fromName;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _logger = logger;
            _apiKey = config["Brevo:ApiKey"] ?? throw new ArgumentNullException(nameof(_apiKey));
            _fromEmail = config["Brevo:FromEmail"] ?? throw new ArgumentNullException(nameof(_fromEmail));
            _fromName = config["Brevo:FromName"] ?? throw new ArgumentNullException(nameof(_fromName));

            Configuration.Default.ApiKey.Clear();
            Configuration.Default.ApiKey.Add("api-key", _apiKey);
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlContent)
        {
            var api = new TransactionalEmailsApi();
            var email = new SendSmtpEmail(
                sender: new SendSmtpEmailSender(_fromName, _fromEmail),
                to: new List<SendSmtpEmailTo> { new(toEmail) },
                subject: subject,
                htmlContent: htmlContent
            );

            try
            {
                var result = await api.SendTransacEmailAsync(email);
                _logger.LogInformation("Email sent to {Email}, messageId: {Id}", toEmail, result.MessageId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
                return false;
            }
        }
    }
}
