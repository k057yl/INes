using Ganss.Xss;
using INest.Data.Entities.Infrastructure;
using INest.Exceptions;
using INest.Infrastructure.Email;
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
        private readonly IHtmlSanitizer _sanitizer;

        public RegisterHandler(
            UserManager<AppUser> userManager,
            IEmailService emailService,
            IStringLocalizer<SharedResource> emailT,
            IHtmlSanitizer sanitizer)
        {
            _userManager = userManager;
            _emailService = emailService;
            _emailT = emailT;
            _sanitizer = sanitizer;
        }

        public async Task Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var normalizedEmail = request.Email.Trim().ToUpperInvariant();
            var sanitizedUsername = _sanitizer.Sanitize(request.Username).Trim();

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
                    EmailConfirmed = false
                };
                var result = await _userManager.CreateAsync(user, request.Password);

                if (!result.Succeeded)
                    throw new AppException(AUTH.ERRORS.REGISTRATION_FAILED, 400);
            }

            var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            user.VerificationCode = code;
            user.VerificationCodeExpiryTime = DateTime.UtcNow.AddMinutes(10);

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                throw new AppException(SYSTEM.DEFAULT_ERROR, 500);

            var subject = _emailT[EMAILS.CONFIRM_SUBJECT];
            var body = string.Format(_emailT[EMAILS.CONFIRM_BODY], code);

            await _emailService.SendEmailAsync(normalizedEmail, subject, body);
        }
    }
}
