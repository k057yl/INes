using INest.Data;
using INest.Models.Entities;
using INest.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace INest.Services
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAppServices(this IServiceCollection services, IConfiguration config)
        {
            // ---------- Localization ----------
            services.AddLocalization(options =>
            {
                options.ResourcesPath = "Resources";
            });

            // ----------- Setting localization for DataAnnotations ---------- 
            services.AddControllers()
                .AddDataAnnotationsLocalization(options =>
                {
                    options.DataAnnotationLocalizerProvider = (type, factory) =>
                        factory.Create(typeof(SharedResource));
                });

            // ---------- DB ----------
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(config.GetConnectionString("DefaultConnection"))
            );

            // ---------- Identity ----------
            services.AddIdentity<AppUser, IdentityRole<Guid>>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            // ---------- JWT ----------
            var jwt = config.GetSection("Jwt");
            var key = jwt["Key"]?.Trim();
            var issuer = jwt["Issuer"]?.Trim();
            var audience = jwt["Audience"]?.Trim();

            if (string.IsNullOrEmpty(key))
                throw new InvalidOperationException("JWT Key missing.");

            if (string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
                throw new InvalidOperationException("JWT Issuer or Audience missing.");

            var keyBytes = Encoding.UTF8.GetBytes(key);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
                };

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";
                        return context.Response.WriteAsync("{\"error\": \"Unauthorized: token invalid or missing.\"}");
                    }
                };
            });

            services.AddAuthorization();

            // ---------- CORS ----------
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAngular", policy =>
                    policy.WithOrigins("http://localhost:4200")
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials());
            });

            services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings"));

            // ---------- Custom services ----------
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IItemService, ItemService>();
            services.AddScoped<ILocationService, LocationService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IPhotoService, PhotoService>();

            return services;
        }
    }
}