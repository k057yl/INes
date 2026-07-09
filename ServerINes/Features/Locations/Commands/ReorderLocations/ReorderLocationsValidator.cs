using FluentValidation;

namespace INest.Features.Locations.Commands.ReorderLocations
{
    public class ReorderLocationsValidator : AbstractValidator<ReorderLocationsCommand>
    {
        public ReorderLocationsValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.OrderedIds)
                .NotEmpty()
                .WithMessage(INest.Constants.LocalizationConstants.ERRORS.EMPTY_LIST);
        }
    }
}
