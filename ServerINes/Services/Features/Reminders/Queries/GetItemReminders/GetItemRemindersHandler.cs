using INest.Models.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace INest.Services.Features.Reminders.Queries.GetItemReminders
{
    public class GetItemRemindersHandler : IRequestHandler<GetItemRemindersQuery, IEnumerable<Reminder>>
    {
        private readonly AppDbContext _context;

        public GetItemRemindersHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Reminder>> Handle(GetItemRemindersQuery request, CancellationToken cancellationToken)
        {
            return await _context.Reminders
                .Where(r => r.ItemId == request.ItemId && r.Item.UserId == request.UserId)
                .AsNoTracking()
                .OrderByDescending(r => r.TriggerAt)
                .ToListAsync(cancellationToken);
        }
    }
}
