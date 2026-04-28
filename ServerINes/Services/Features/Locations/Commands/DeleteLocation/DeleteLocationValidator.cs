using FluentValidation;

namespace INest.Services.Features.Locations.Commands.DeleteLocation
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
