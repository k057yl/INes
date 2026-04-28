using Microsoft.AspNetCore.Identity;

namespace INest.Services.Infrastructure
{
    public class CustomUserValidator<TUser> : UserValidator<TUser> where TUser : class
    {
        public override async Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user)
        {
            var result = await base.ValidateAsync(manager, user);
            var errors = result.Succeeded ? new List<IdentityError>() : result.Errors.ToList();

            errors = errors.Where(e => e.Code != "DuplicateUserName").ToList();

            return errors.Any() ? IdentityResult.Failed(errors.ToArray()) : IdentityResult.Success;
        }
    }
}