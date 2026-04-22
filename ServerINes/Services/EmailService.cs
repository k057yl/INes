using brevo_csharp.Api;
using brevo_csharp.Model;
using Ganss.Xss;
using INest.Constants;
using INest.Exceptions;
using INest.Services.Interfaces;
using Microsoft.Extensions.Localization;
using Configuration = brevo_csharp.Client.Configuration;
using Task = System.Threading.Tasks.Task;

namespace INest.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly string _apiKey;
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly Configuration _brevoConfig;
        private readonly IHtmlSanitizer _sanitizer;

        public EmailService(IConfiguration config, ILogger<EmailService> logger, IStringLocalizer<SharedResource> localizer, IHtmlSanitizer sanitizer)
        {
            _logger = logger;
            _localizer = localizer;
            _apiKey = config["Brevo:ApiKey"] ?? throw new ArgumentNullException("Brevo ApiKey missing");
            _fromEmail = config["Brevo:FromEmail"] ?? throw new ArgumentNullException("Brevo FromEmail missing");
            _fromName = config["Brevo:FromName"] ?? throw new ArgumentNullException("Brevo FromName missing");
            _sanitizer = sanitizer;

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

        public async Task SendLendingNotificationAsync(string toEmail, string itemName, string personName, DateTime? returnDate, bool isBorrowedByMe)
        {
            string subjectKey = isBorrowedByMe ? "LENDING_SUBJECT_IN" : "LENDING_SUBJECT_OUT";
            string bodyKey = isBorrowedByMe ? "LENDING_BODY_IN" : "LENDING_BODY_OUT";

            string subject = _localizer[subjectKey].Value;
            string dateStr = returnDate?.ToString("dd.MM.yyyy") ?? _localizer["COMMON_NOT_SPECIFIED"].Value;

            var safeItemName = _sanitizer.Sanitize(itemName);
            var safePersonName = _sanitizer.Sanitize(personName);

            string htmlContent = string.Format(_localizer[bodyKey].Value, safeItemName, safePersonName, dateStr);

            await SendEmailAsync(toEmail, subject, htmlContent);
        }

        public async Task SendReminderNotificationAsync(string toEmail, string title, DateTime triggerAt)
        {
            string subject = _localizer["REMINDER_SUBJECT"].Value;
            string dateStr = triggerAt.ToString("dd.MM.yyyy");
            string htmlContent = string.Format(_localizer["REMINDER_BODY"].Value, dateStr, title);

            await SendEmailAsync(toEmail, subject, htmlContent);
        }
    }
}