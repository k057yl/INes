using MediatR;

namespace INest.Features.Feedback.Commands.RateFeedback
{
    public class RateFeedbackHandler : IRequestHandler<RateFeedbackCommand>
    {
        private readonly AppDbContext _context;

        public RateFeedbackHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task Handle(RateFeedbackCommand request, CancellationToken cancellationToken)
        {
            var feedback = await _context.Feedbacks.FindAsync(new object[] { request.FeedbackId }, cancellationToken);

            if (feedback == null) return;

            feedback.Rating = Math.Clamp(request.Rating, 1, 5);

            if (!string.IsNullOrWhiteSpace(request.MissingFeatures))
            {
                feedback.MissingFeatures = request.MissingFeatures.Trim();
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
