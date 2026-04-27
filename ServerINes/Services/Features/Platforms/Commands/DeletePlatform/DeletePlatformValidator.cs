using FluentValidation;

namespace INest.Services.Features.Platforms.Commands.DeletePlatform
{
    public class DeletePlatformValidator : AbstractValidator<DeletePlatformCommand>
    {
        public DeletePlatformValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.PlatformId).NotEmpty();
        }
    }
}
