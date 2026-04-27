using FluentValidation;

namespace INest.Services.Features.Items.Commands.DeleteItem
{
    public class DeleteItemValidator : AbstractValidator<DeleteItemCommand>
    {
        public DeleteItemValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.ItemId).NotEmpty();
        }
    }
}
