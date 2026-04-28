using INest.Constants;
using INest.Middleware;
using INest.Models.Entities;
using INest.Seeders;
using INest.Services;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// ---------- Регистрация слоев ----------
builder.Services.AddInfrastructure(builder.Configuration);

// Здесь живут сервисы и их декораторы для кэширования
builder.Services.AddBusinessServices();

// ---------- Swagger ----------
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

app.Use(async (context, next) =>
{
    context.Response.Headers.Append("Content-Security-Policy", SharedConstants.CONTENT_SECURITY_POLICY);
    await next();
});

app.UseCors("AllowAngular");
app.UseHttpsRedirection();

// ---------- Миграции + сидирование ----------
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

    await AdminSeeder.SeedAsync(userManager, roleManager, config);
}

// ---------- Pipeline ----------
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();