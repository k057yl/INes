using DotNetEnv;
using INest;
using INest.Constants;
using INest.Data.Entities.Infrastructure;
using INest.Middleware;
using INest.Seeders;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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

app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var dbContext = services.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();

    var userManager = services.GetRequiredService<UserManager<AppUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
    var config = services.GetRequiredService<IConfiguration>();

    await AdminSeeder.SeedAsync(userManager, roleManager, config);
}

app.MapControllers();

app.Run();