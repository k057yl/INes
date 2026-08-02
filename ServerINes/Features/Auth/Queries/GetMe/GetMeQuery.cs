using INest.Features.Auth.DTOs;
using MediatR;

namespace INest.Features.Auth.Queries.GetMe
{
    public record GetMeQuery(string UserId) : IRequest<GetMeResponseDto>;
}
