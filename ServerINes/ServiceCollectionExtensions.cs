using CloudinaryDotNet;
using FluentValidation;
using Ganss.Xss;
using INest.Constants;
using INest.Data.Entities;
using INest.Data.Entities.Infrastructure;
using INest.Features.Reminders.Services;
using INest.Infrastructure;
using INest.Infrastructure.BackgroundServices.Cleanup;
using INest.Infrastructure.BackgroundServices.Reminder;
using INest.Infrastructure.BackgroundServices.Telegram;
using INest.Infrastructure.Behaviors;
using INest.Infrastructure.Dispatcher;
using INest.Infrastructure.Email;
using INest.Infrastructure.Identity;
using INest.Infrastructure.QrCode;
using INest.Infrastructure.Sanitizer;
using INest.Infrastructure.Storage;
using INest.Infrastructure.Time;
using INest.Infrastructure.Tracker;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace INest
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

            services.AddSingleton(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<CloudinarySettings>>().Value;
                var acc = new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret);
                return new Cloudinary(acc);
            });

            return services;
        }

        private static void AddCustomControllers(this IServiceCollection services)
        {
            services.AddControllers()
                .AddApplicationPart(typeof(Controllers.AuthController).Assembly)
                .AddJsonOptions(options => {
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                });
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
                options.User.RequireUniqueEmail = true;

                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders()
            .AddUserValidator<CustomUserValidator<AppUser>>();
        }

        private static void AddCustomAuth(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
                options.OnAppendCookie = cookieContext => CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
                options.OnDeleteCookie = cookieContext => CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
            });

            var jwt = config.GetSection("Jwt");
            var keyBytes = Encoding.UTF8.GetBytes(jwt["Key"]?.Trim() ?? throw new InvalidOperationException(SharedConstants.JWT_KEY_MISSING));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = true;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt["Issuer"]?.Trim(),
                    ValidAudience = jwt["Audience"]?.Trim(),
                    IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                    ClockSkew = TimeSpan.FromSeconds(30)
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        string authHeader = context.Request.Headers["Authorization"].ToString();
                        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                        {
                            context.Token = authHeader.Substring("Bearer ".Length).Trim();
                            return Task.CompletedTask;
                        }

                        if (context.Request.Cookies.ContainsKey("X-Access-Token"))
                        {
                            context.Token = context.Request.Cookies["X-Access-Token"];
                        }

                        return Task.CompletedTask;
                    }
                };
            });
        }

        private static void CheckSameSite(HttpContext httpContext, CookieOptions options)
        {
            if (options.SameSite == SameSiteMode.None)
            {
                var userAgent = httpContext.Request.Headers["User-Agent"].ToString();
                options.Secure = true;
            }
        }

        private static void AddCustomCors(this IServiceCollection services, IConfiguration config)
        {
            var allowedOrigins = config.GetSection("AllowedOrigins").Get<string[]>() ?? new[]
            {
                SharedConstants.LOCALHOST,
                SharedConstants.LOCALHOST_HTTPS,
                SharedConstants.PWA,
                SharedConstants.PWA_HTTPS,
                SharedConstants.PWA_FROM_IP,
                SharedConstants.PWA_MOBILE,
                SharedConstants.PWA_MOBILE_HTTPS,
                SharedConstants.PROD_ORIGIN,
                SharedConstants.PROD_ORIGIN_HTTPS
            };

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAngular", policy =>
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials());
            });
        }

        public static IServiceCollection AddBusinessServices(this IServiceCollection services)
        {
            services.AddValidatorsFromAssembly(typeof(ServiceCollectionExtensions).Assembly);

            services.AddSingleton<ICacheTracker, CacheTracker>();
            services.AddSingleton<ISanitizerService, SanitizerService>();
            services.AddSingleton<IUserTimeService, UserTimeService>();
            services.AddSingleton<IReminderScheduler, ReminderScheduler>();

            services.AddTransient<IQrCodeService, QrCodeService>();

            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<INotificationDispatcher, NotificationDispatcher>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IPhotoService, PhotoService>();
            services.AddScoped<LendingService>();

            services.AddScoped<IReminderProcessor, ReminderProcessor>();

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly);
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            });

            // Background Services
            services.AddHostedService<ReminderWorker>();
            services.AddHostedService<UnconfirmedUserCleanupWorker>();
            services.AddHostedService<TelegramBotBackgroundService>();

            return services;
        }
    }
}