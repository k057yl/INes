using FluentValidation;
using static INest.Constants.LocalizationConstants;

namespace INest.Features.Auth.Commands.ResetPassword
{
    public class ResetPasswordValidator : AbstractValidator<ResetPasswordCommand>
    {
        public ResetPasswordValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(ERRORS.REQUIRED_FIELD)
                .EmailAddress().WithMessage(ERRORS.INVALID_EMAIL_FORMAT);

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage(ERRORS.REQUIRED_FIELD)
                .MinimumLength(6).WithMessage(ERRORS.PWD_MIN_LENGTH)
                .Matches(@"^[\u0000-\u007F]+$").WithMessage(ERRORS.PWD_LATIN)
                .Matches(@"[A-Z]").WithMessage(ERRORS.PWD_UPPER)
                .Matches(@"[0-9]").WithMessage(ERRORS.PWD_DIGIT)
                .Matches(@"[^a-zA-Z0-9]").WithMessage(ERRORS.PWD_SPEC);

            RuleFor(x => x.Token)
                .NotEmpty().WithMessage(AUTH.ERRORS.TOKEN_MISSING);
        }
    }
}
