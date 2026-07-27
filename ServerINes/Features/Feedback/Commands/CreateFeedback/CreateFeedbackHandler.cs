using INest.Exceptions;
using INest.Infrastructure.Sanitizer;
using MediatR;
using static INest.Constants.LocalizationConstants;

namespace INest.Features.Feedback.Commands.CreateFeedback
{
    public class CreateFeedbackHandler : IRequestHandler<CreateFeedbackCommand, Guid>
    {
        private readonly AppDbContext _context;
        private readonly ISanitizerService _sanitizer;

        public CreateFeedbackHandler(AppDbContext context, ISanitizerService sanitizer)
        {
            _context = context;
            _sanitizer = sanitizer;
        }

        public async Task<Guid> Handle(CreateFeedbackCommand request, CancellationToken cancellationToken)
        {
            var cleanMessage = _sanitizer.StripAllHtml(request.Message).Trim();

            if (string.IsNullOrWhiteSpace(cleanMessage))
                throw new AppException(FEEDBACK.ERRORS.MESSAGE_EMPTY, 400);

            var feedback = new INest.Data.Entities.Infrastructure.Feedback
            {
                UserId = request.UserId,
                Type = request.Type,
                Message = cleanMessage,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Feedbacks.AddAsync(feedback, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return feedback.Id;
        }
    }
}
