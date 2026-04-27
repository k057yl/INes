using FluentValidation;

namespace INest.Services.Features.Locations.Commands.UpdateLocationSortOrder
{
    public class UpdateLocationSortOrderValidator : AbstractValidator<UpdateLocationSortOrderCommand>
    {
        public UpdateLocationSortOrderValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.LocationId).NotEmpty();
            RuleFor(x => x.NewOrder).GreaterThanOrEqualTo(0);
        }
    }
}
