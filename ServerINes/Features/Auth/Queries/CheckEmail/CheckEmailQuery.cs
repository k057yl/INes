using MediatR;

namespace INest.Features.Auth.Queries.CheckEmail
{
    public record CheckEmailQuery(string Email) : IRequest<bool>;
}
