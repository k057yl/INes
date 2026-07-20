using FluentValidation;

namespace INest.Features.Items.Commands.DeleteArchivedItem
{
    public class DeleteArchivedItemCommandValidator : AbstractValidator<DeleteArchivedItemCommand>
    {
        public DeleteArchivedItemCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty();

            RuleFor(x => x.ItemId)
                .NotEmpty();
        }
    }
}
