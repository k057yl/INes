using FluentValidation;
using Ganss.Xss;
using MediatR;
using INest.Constants;
using INest.Models.Entities;
using INest.Models.Validators;
using INest.Services.BackgroundServices;
using INest.Services.Behaviors;
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

            services.AddSingleton<ICacheTracker, CacheTracker>();
            services.AddSingleton<IHtmlSanitizer, HtmlSanitizer>();

            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IPhotoService, PhotoService>();
            services.AddScoped<IAuthService, AuthService>();

            services.AddScoped<ILendingService, LendingService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<ILocationService, LocationService>();
            services.AddScoped<IPlatformService, PlatformService>();
            services.AddScoped<ISalesService, SalesService>();
            services.AddScoped<IReminderService, ReminderService>();
            //services.AddScoped<IItemService, ItemService>();

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly);

                // 2. Регистрация наших универсальных Декораторов (Pipeline Behaviors)
                // Порядок ВАЖЕН: запрос сначала попадает в кеш, если там пусто -> идет в валидацию -> потом в хендлер
                // Раскомментируем, когда создадим эти классы:
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            });

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