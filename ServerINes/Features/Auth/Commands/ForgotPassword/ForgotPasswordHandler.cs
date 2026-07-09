using INest.Constants;
using INest.Data.Entities.Infrastructure;
using INest.Infrastructure.Email;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using static INest.Constants.LocalizationConstants;

namespace INest.Features.Auth.Commands.ForgotPassword
{
    public class ForgotPasswordHandler : IRequestHandler<ForgotPasswordCommand>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;
        private readonly IStringLocalizer<SharedResource> _emailT;

        public ForgotPasswordHandler(
            UserManager<AppUser> userManager,
            IEmailService emailService,
            IConfiguration config,
            IStringLocalizer<SharedResource> emailT)
        {
            _userManager = userManager;
            _emailService = emailService;
            _config = config;
            _emailT = emailT;
        }

        public async Task Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null) return;

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var baseUrl = _config["Frontend:Url"];
            var pathTemplate = _config["Frontend:ResetPasswordPath"] ?? SharedConstants.DEFAULT_RESET_PASSWORD_PATH;

            var callbackUrl = string.Format(pathTemplate, baseUrl, request.Email, Uri.EscapeDataString(token));

            var subject = _emailT[EMAILS.RESET_SUBJECT];
            var body = string.Format(_emailT[EMAILS.RESET_BODY], callbackUrl);

            await _emailService.SendEmailAsync(request.Email, subject, body);
        }
    }
}
