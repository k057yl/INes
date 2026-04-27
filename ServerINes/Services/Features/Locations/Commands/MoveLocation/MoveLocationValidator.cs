using FluentValidation;

namespace INest.Services.Features.Locations.Commands.MoveLocation
{
    public class MoveLocationValidator : AbstractValidator<MoveLocationCommand>
    {
        public MoveLocationValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.LocationId).NotEmpty();
        }
    }
}
