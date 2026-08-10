using INest.Data.Entities.Infrastructure;
using INest.Exceptions;
using INest.Features.Auth.DTOs;
using MediatR;
using Microsoft.AspNetCore.Identity;
using static INest.Constants.LocalizationConstants;

namespace INest.Features.Auth.Queries.GetMe
{
    public class GetMeHandler : IRequestHandler<GetMeQuery, GetMeResponseDto>
    {
        private readonly UserManager<AppUser> _userManager;

        public GetMeHandler(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<GetMeResponseDto> Handle(GetMeQuery request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);

            if (user == null)
                throw new AppException(AUTH.ERRORS.INVALID_TOKEN, 401);

            var roles = await _userManager.GetRolesAsync(user);

            return new GetMeResponseDto
            {
                Email = user.Email ?? "",
                DisplayName = !string.IsNullOrWhiteSpace(user.DisplayName)
                    ? user.DisplayName
                    : (user.Email?.Split('@')[0] ?? "User"),
                Roles = roles.ToList(),
                CompletedTutorials = (int)user.CompletedTutorials
            };
        }
    }
}
