using INest.Exceptions;
using MediatR;
using static INest.Constants.LocalizationConstants;

namespace INest.Features.Feedback.Commands.ToggleProcessed
{
    public class ToggleProcessedHandler : IRequestHandler<ToggleProcessedCommand>
    {
        private readonly AppDbContext _context;

        public ToggleProcessedHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task Handle(ToggleProcessedCommand request, CancellationToken cancellationToken)
        {
            var feedback = await _context.Feedbacks.FindAsync(new object[] { request.FeedbackId }, cancellationToken);

            if (feedback == null)
                throw new AppException(FEEDBACK.ERRORS.NOT_FOUND);

            feedback.IsProcessed = !feedback.IsProcessed;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
