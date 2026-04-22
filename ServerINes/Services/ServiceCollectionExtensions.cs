using FluentValidation;
using Ganss.Xss;
using INest.Constants;
using INest.Models.Entities;
using INest.Models.Validators;
using INest.Services.BackgroundServices;
using INest.Services.Decorator;
using INest.Services.Interfaces;
using INest.Services.Tracker;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace INest.Services
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            services.AddCustomControllers();
            services.AddCustomDatabase(config);
            services.AddCustomIdentity(config);
            services.AddCustomAuth(config);
            services.AddCustomCors(config);

            services.AddMemoryCache();
            services.AddLocalization(options => options.ResourcesPath = "Resources");
            services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings"));

            return services;
        }

        private static void AddCustomControllers(this IServiceCollection services)
        {
            services.AddControllers()
                .AddApplicationPart(typeof(INest.Controllers.AuthController).Assembly)
                .AddJsonOptions(options => {
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                });

            services.AddValidatorsFromAssemblyContaining<CategoryRules>();
        }

        private static void AddCustomDatabase(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(config.GetConnectionString("DefaultConnection"))
            );
        }

        private static void AddCustomIdentity(this IServiceCollection services, IConfiguration config)
        {
            services.AddIdentity<AppUser, IdentityRole<Guid>>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();
        }

        private static void AddCustomAuth(this IServiceCollection services, IConfiguration config)
        {
            var jwt = config.GetSection("Jwt");
            var keyBytes = Encoding.UTF8.GetBytes(jwt["Key"]?.Trim() ?? throw new InvalidOperationException(SharedConstants.JWT_KEY_MISSING));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt["Issuer"],
                    ValidAudience = jwt["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (context.Request.Cookies.ContainsKey("X-Access-Token"))
                        {
                            context.Token = context.Request.Cookies["X-Access-Token"];
                        }
                        return Task.CompletedTask;
                    }
                };
            });
        }

        private static void AddCustomCors(this IServiceCollection services, IConfiguration config)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAngular", policy =>
                    policy.WithOrigins(
                        SharedConstants.LOCALHOST,
                        SharedConstants.PWA,
                        SharedConstants.PWA_FROM_IP,
                        SharedConstants.PWA_MOBILE,
                        SharedConstants.WSL_IP
                    )
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });
        }

        public static IServiceCollection AddBusinessServices(this IServiceCollection services)
        {
            services.AddValidatorsFromAssemblyContaining<CategoryRules>();

            // Singleton
            services.AddSingleton<ICacheTracker, CacheTracker>();
            services.AddSingleton<IHtmlSanitizer, HtmlSanitizer>();

            // Scoped
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IPhotoService, PhotoService>();
            services.AddScoped<IAuthService, AuthService>();

            // Decor
            services.AddDecoratedService<ICategoryService, CategoryService, CachedCategoryService>();
            services.AddDecoratedService<ILocationService, LocationService, CachedLocationService>();
            services.AddDecoratedService<IPlatformService, PlatformService, CachedPlatformService>();
            services.AddDecoratedService<ISalesService, SalesService, CachedSalesService>();
            services.AddDecoratedService<IItemService, ItemService, CachedItemService>();
            services.AddDecoratedService<IReminderService, ReminderService, CachedReminderService>();
            services.AddDecoratedService<ILendingService, LendingService, CachedLendingService>();

            services.AddHostedService<ReminderWorker>();

            return services;
        }

        private static void AddDecoratedService<TInterface, TService, TDecorator>(this IServiceCollection services)
            where TInterface : class
            where TService : class, TInterface
            where TDecorator : class, TInterface
        {
            services.AddScoped<TService>();
            services.AddScoped<TInterface>(sp =>
            {
                var inner = sp.GetRequiredService<TService>();
                return ActivatorUtilities.CreateInstance<TDecorator>(sp, inner);
            });
        }
    }
}