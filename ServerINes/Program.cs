using INest;
using INest.Constants;
using INest.Data.Entities.Infrastructure;
using INest.Middleware;
using INest.Seeders;
using Microsoft.AspNetCore.Identity;
using DotNetEnv;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddBusinessServices();

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

app.UseRouting();

app.UseCors("AllowAngular");

app.Use(async (context, next) =>
{
    context.Response.Headers.Append("Content-Security-Policy", SharedConstants.CONTENT_SECURITY_POLICY);
    await next();
});

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

    await AdminSeeder.SeedAsync(userManager, roleManager, config);
}

app.MapControllers();

app.Run();