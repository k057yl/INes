using INest.Data.Entities.Infrastructure;
using INest.Infrastructure.Email;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using System.Security.Cryptography;
using static INest.Constants.LocalizationConstants;

namespace INest.Features.Auth.Commands.ForgotPassword
{
    public class ForgotPasswordHandler : IRequestHandler<ForgotPasswordCommand>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IStringLocalizer<SharedResource> _emailT;

        public ForgotPasswordHandler(
            UserManager<AppUser> userManager,
            IEmailService emailService,
            IStringLocalizer<SharedResource> emailT)
        {
            _userManager = userManager;
            _emailService = emailService;
            _emailT = emailT;
        }

        public async Task Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            var normalizedEmail = request.Email.Trim().ToUpperInvariant();
            var user = await _userManager.FindByEmailAsync(normalizedEmail);

            if (user == null) return;

            var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            user.VerificationCode = code;
            user.VerificationCodeExpiryTime = DateTime.UtcNow.AddMinutes(10);

            await _userManager.UpdateAsync(user);

            var subject = _emailT[EMAILS.RESET_SUBJECT];
            var body = string.Format(_emailT[EMAILS.RESET_BODY], code);

            await _emailService.SendEmailAsync(normalizedEmail, subject, body);
        }
    }
}