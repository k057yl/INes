using INest.Constants;
using INest.Models.Entities;
using INest.Services.Decorator;
using INest.Services.Interfaces;
using INest.Models.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
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
                .AddJsonOptions(options => {
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                });

            services.AddFluentValidationAutoValidation();
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
            var keyBytes = Encoding.UTF8.GetBytes(jwt["Key"]?.Trim() ?? throw new InvalidOperationException("JWT Key missing"));

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
                    IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
                };
            });
        }

        private static void AddCustomCors(this IServiceCollection services, IConfiguration config)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAngular", policy =>
                    policy.WithOrigins(SharedConstants.LOCALHOST)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials());
            });
        }

        public static IServiceCollection AddBusinessServices(this IServiceCollection services)
        {
            // Базовые сервисы
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IPhotoService, PhotoService>();
            services.AddScoped<IAuthService, AuthService>();

            // Регистрация декораторов (вынесено в хелпер для читаемости)
            services.AddDecoratedService<ICategoryService, CategoryService, CachedCategoryService>();
            services.AddDecoratedService<ILocationService, LocationService, CachedLocationService>();
            services.AddDecoratedService<IPlatformService, PlatformService, CachedPlatformService>();
            services.AddDecoratedService<ISalesService, SalesService, CachedSalesService>();
            services.AddDecoratedService<IItemService, ItemService, CachedItemService>();

            return services;
        }

        // Хелпер для регистрации декораторов, чтобы не дублировать код
        private static void AddDecoratedService<TInterface, TService, TDecorator>(this IServiceCollection services)
            where TInterface : class
            where TService : class, TInterface
            where TDecorator : class, TInterface
        {
            services.AddScoped<TService>();
            services.AddScoped<TInterface>(sp =>
            {
                var inner = sp.GetRequiredService<TService>();
                var cache = sp.GetRequiredService<IMemoryCache>();
                return (TDecorator)Activator.CreateInstance(typeof(TDecorator), inner, cache)!;
            });
        }
    }
}