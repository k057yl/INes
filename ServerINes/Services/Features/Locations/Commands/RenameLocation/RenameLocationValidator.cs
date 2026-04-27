using FluentValidation;

namespace INest.Services.Features.Locations.Commands.RenameLocation
{
    public class RenameLocationValidator : AbstractValidator<RenameLocationCommand>
    {
        public RenameLocationValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.LocationId).NotEmpty();
            RuleFor(x => x.NewName).NotEmpty();
        }
    }
}
