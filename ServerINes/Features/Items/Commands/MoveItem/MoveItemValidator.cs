using FluentValidation;

namespace INest.Features.Items.Commands.MoveItem
{
    public class MoveItemValidator : AbstractValidator<MoveItemCommand>
    {
        public MoveItemValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.ItemId).NotEmpty();
        }
    }
}