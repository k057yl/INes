using INest.Data.Entities.Infrastructure;
using INest.Exceptions;
using INest.Infrastructure.Email;
using INest.Infrastructure.Sanitizer;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using System.Security.Cryptography;
using static INest.Constants.LocalizationConstants;

namespace INest.Features.Auth.Commands.Register
{
    public class RegisterHandler : IRequestHandler<RegisterCommand>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IStringLocalizer<SharedResource> _emailT;
        private readonly ISanitizerService _sanitizer;

        public RegisterHandler(
            UserManager<AppUser> userManager,
            IEmailService emailService,
            IStringLocalizer<SharedResource> emailT,
            ISanitizerService sanitizer)
        {
            _userManager = userManager;
            _emailService = emailService;
            _emailT = emailT;
            _sanitizer = sanitizer;
        }

        public async Task Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var normalizedEmail = request.Email.Trim().ToUpperInvariant();

            var sanitizedUsername = _sanitizer.StripAllHtml(request.Username).Trim();

            if (string.IsNullOrWhiteSpace(sanitizedUsername))
                throw new AppException(AUTH.ERRORS.INVALID_USERNAME, 400);

            var user = await _userManager.FindByEmailAsync(normalizedEmail);

            if (user != null && user.EmailConfirmed)
                throw new AppException(AUTH.ERRORS.EMAIL_ALREADY_EXISTS, 400);

            if (user == null)
            {
                user = new AppUser
                {
                    Email = normalizedEmail,
                    UserName = normalizedEmail,
                    DisplayName = sanitizedUsername,
                    EmailConfirmed = false,
                    TimeZoneId = string.IsNullOrWhiteSpace(request.TimeZoneId) ? "Europe/Kyiv" : request.TimeZoneId
                };
                var result = await _userManager.CreateAsync(user, request.Password);

                if (!result.Succeeded)
                    throw new AppException(AUTH.ERRORS.REGISTRATION_FAILED, 400);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(request.TimeZoneId))
                {
                    user.TimeZoneId = request.TimeZoneId;
                }
            }

            var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            user.VerificationCode = code;
            user.VerificationCodeExpiryTime = DateTime.UtcNow.AddMinutes(10);

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                throw new AppException(SYSTEM.DEFAULT_ERROR, 500);

            string subject = _emailT[EMAILS.CONFIRM_SUBJECT].Value;
            string bodyTemplate = _emailT[EMAILS.CONFIRM_BODY].Value;

            string htmlBody = string.Format(bodyTemplate, code);

            await _emailService.SendEmailAsync(normalizedEmail, subject, htmlBody);
        }
    }
}