using FluentValidation;

namespace INest.Services.Features.Locations.Commands.CreateLocation
{
    public class CreateLocationValidator : AbstractValidator<CreateLocationCommand>
    {
        public CreateLocationValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.Dto.Name).NotEmpty();
        }
    }
}
