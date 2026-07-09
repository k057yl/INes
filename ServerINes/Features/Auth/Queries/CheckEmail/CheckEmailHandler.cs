using INest.Data.Entities.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace INest.Features.Auth.Queries.CheckEmail
{
    public class CheckEmailHandler : IRequestHandler<CheckEmailQuery, bool>
    {
        private readonly UserManager<AppUser> _userManager;

        public CheckEmailHandler(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<bool> Handle(CheckEmailQuery request, CancellationToken cancellationToken)
        {
            return (await _userManager.FindByEmailAsync(request.Email)) == null;
        }
    }
}
