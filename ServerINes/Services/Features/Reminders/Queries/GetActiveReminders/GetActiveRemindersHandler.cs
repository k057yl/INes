using INest.Models.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace INest.Services.Features.Reminders.Queries.GetActiveReminders
{
    public class GetActiveRemindersHandler : IRequestHandler<GetActiveRemindersQuery, IEnumerable<Reminder>>
    {
        private readonly AppDbContext _context;

        public GetActiveRemindersHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Reminder>> Handle(GetActiveRemindersQuery request, CancellationToken cancellationToken)
        {
            return await _context.Reminders
                .Where(r => r.Item.UserId == request.UserId && !r.IsCompleted)
                .AsNoTracking()
                .OrderBy(r => r.TriggerAt)
                .ToListAsync(cancellationToken);
        }
    }
}
