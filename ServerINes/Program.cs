using INest.Models.Entities;
using INest.Seeders;
using INest.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

// ---------- Services ----------
builder.Services.AddAppServices(builder.Configuration);

// ---------- Controllers ----------
builder.Services.AddControllers();

// ---------- Swagger ----------
builder.Services.AddOpenApi();

var app = builder.Build();

// ---------- Localization setup ----------
var supportedCultures = new[] { "en", "uk", "ru" };

var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("en")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

localizationOptions.RequestCultureProviders = new IRequestCultureProvider[]
{
    new QueryStringRequestCultureProvider(),
    new AcceptLanguageHeaderRequestCultureProvider()
};

// ---------- Миграции + сидирование ----------
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider
        .GetRequiredService<UserManager<AppUser>>();

    var roleManager = scope.ServiceProvider
        .GetRequiredService<RoleManager<IdentityRole<Guid>>>();

    await AdminSeeder.SeedAsync(userManager, roleManager);
}

// ---------- Pipeline ----------
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseRequestLocalization(localizationOptions);

app.UseCors("AllowAngular");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();