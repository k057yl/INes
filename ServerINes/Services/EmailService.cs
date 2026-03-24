using brevo_csharp.Api;
using brevo_csharp.Model;
using INest.Services.Interfaces;
using INest.Constants;
using INest.Exceptions;
using Configuration = brevo_csharp.Client.Configuration;

namespace INest.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly string _apiKey;
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly Configuration _brevoConfig;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _logger = logger;
            _apiKey = config["Brevo:ApiKey"] ?? throw new ArgumentNullException("Brevo ApiKey missing");
            _fromEmail = config["Brevo:FromEmail"] ?? throw new ArgumentNullException("Brevo FromEmail missing");
            _fromName = config["Brevo:FromName"] ?? throw new ArgumentNullException("Brevo FromName missing");

            _brevoConfig = new Configuration();
            _brevoConfig.ApiKey.Add("api-key", _apiKey);
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlContent)
        {
            var api = new TransactionalEmailsApi(_brevoConfig);

            var sendSmtpEmail = new SendSmtpEmail(
                sender: new SendSmtpEmailSender(_fromName, _fromEmail),
                to: new List<SendSmtpEmailTo> { new SendSmtpEmailTo(toEmail) },
                subject: subject,
                htmlContent: htmlContent
            );

            try
            {
                var result = await api.SendTransacEmailAsync(sendSmtpEmail);
                _logger.LogInformation("Email sent to {Email}, messageId: {Id}", toEmail, result.MessageId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", toEmail);

                throw new AppException(LocalizationConstants.SYSTEM.EMAIL_SEND_FAILED, 500);
            }
        }
    }
}