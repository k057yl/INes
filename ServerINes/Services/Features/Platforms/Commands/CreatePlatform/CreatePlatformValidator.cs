using FluentValidation;

namespace INest.Services.Features.Platforms.Commands.CreatePlatform
{
    public class CreatePlatformValidator : AbstractValidator<CreatePlatformCommand>
    {
        public CreatePlatformValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.Dto.Name).NotEmpty();
        }
    }
}
