using INest.Data.Entities.Infrastructure;
using INest.Exceptions;
using INest.Infrastructure.Email;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using System.Security.Cryptography;
using static INest.Constants.LocalizationConstants;

namespace INest.Features.Auth.Commands.ResendCode
{
    public class ResendCodeHandler : IRequestHandler<ResendCodeCommand>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IStringLocalizer<SharedResource> _emailT;

        public ResendCodeHandler(
            UserManager<AppUser> userManager,
            IEmailService emailService,
            IStringLocalizer<SharedResource> emailT)
        {
            _userManager = userManager;
            _emailService = emailService;
            _emailT = emailT;
        }

        public async Task Handle(ResendCodeCommand request, CancellationToken cancellationToken)
        {
            var normalizedEmail = request.Email.Trim().ToUpperInvariant();
            var user = await _userManager.FindByEmailAsync(normalizedEmail);

            if (user == null || user.EmailConfirmed)
                throw new AppException(AUTH.ERRORS.USER_NOT_FOUND, 400);

            var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            user.VerificationCode = code;
            user.VerificationCodeExpiryTime = DateTime.UtcNow.AddMinutes(10);

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded) throw new AppException(SYSTEM.DEFAULT_ERROR, 500);

            var subject = _emailT[EMAILS.CONFIRM_SUBJECT];
            var body = string.Format(_emailT[EMAILS.CONFIRM_BODY], code);

            await _emailService.SendEmailAsync(normalizedEmail, subject, body);
        }
    }
}
