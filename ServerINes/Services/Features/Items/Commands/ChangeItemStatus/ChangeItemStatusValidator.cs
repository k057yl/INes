using FluentValidation;

namespace INest.Services.Features.Items.Commands.ChangeItemStatus
{
    public class ChangeItemStatusValidator : AbstractValidator<ChangeItemStatusCommand>
    {
        public ChangeItemStatusValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.ItemId).NotEmpty();
            RuleFor(x => x.NewStatus).IsInEnum();
        }
    }
}