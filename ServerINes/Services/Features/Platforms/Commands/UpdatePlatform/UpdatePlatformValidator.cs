using FluentValidation;

namespace INest.Services.Features.Platforms.Commands.UpdatePlatform
{
    public class UpdatePlatformValidator : AbstractValidator<UpdatePlatformCommand>
    {
        public UpdatePlatformValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.PlatformId).NotEmpty();
            RuleFor(x => x.Dto.Name).NotEmpty();
        }
    }
}
