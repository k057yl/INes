using FluentValidation;

namespace INest.Features.Locations.Commands.DeleteLocation
{
    public class DeleteLocationValidator : AbstractValidator<DeleteLocationCommand>
    {
        public DeleteLocationValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.Id).NotEmpty();
        }
    }
}
