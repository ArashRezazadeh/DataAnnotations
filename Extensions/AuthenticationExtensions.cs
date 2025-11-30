


using DataAnnotations.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace DataAnnotations.Extensions; 

public static class AuthenticationExtensions
{
    public static IServiceCollection AddCookieAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        }).AddCookie(options =>
        {
            options.Cookie.Name = "Auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
            options.SlidingExpiration = true;
            options.Events = new CookieAuthenticationEvents
            {
                OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                },
                OnRedirectToAccessDenied = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                }
            };
        });

        return services;
    }
    
    public static IServiceCollection AddJwtAuthenticationWithKeyFile(
        this IServiceCollection services, 
        IConfiguration configuration, 
        IWebHostEnvironment environment,
        string keyFileName = "jwt-key.txt")
    {
        // Configure JwtSettings from configuration section
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

        // Read JWT key from file
        string keyPath = Path.Combine(environment.ContentRootPath, keyFileName);
        string jwtKey = File.ReadAllText(keyPath).Trim();
        Console.WriteLine($"jwtKey is {jwtKey}");

        // Create JwtSettings instance
        var jwtSettings = new JwtSettings
        {
            Key = jwtKey,
            Issuer = configuration["Jwt:Issuer"],
            Audience = configuration["Jwt:Audience"],
            ExpirationInMinutes = int.Parse(configuration["Jwt:ExpirationInMinutes"] ?? "60")
        };

        // Validate JWT settings
        ValidateJwtSettings(jwtSettings);

        Console.WriteLine($"JwtSettings created. Key length: {jwtSettings.Key.Length}, " +
                         $"Issuer: {jwtSettings.Issuer}, Audience: {jwtSettings.Audience}");

        // Register as singleton
        services.AddSingleton(jwtSettings);

        // Configure for IOptions pattern
        services.Configure<JwtSettings>(options =>
        {
            options.Key = jwtSettings.Key;
            options.Issuer = jwtSettings.Issuer;
            options.Audience = jwtSettings.Audience;
            options.ExpirationInMinutes = jwtSettings.ExpirationInMinutes;
        });

        // Add JWT authentication
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
                };
            });

        Console.WriteLine($"AddAuthentication: JWT Key length used for IssuerSigningKey: {jwtSettings.Key.Length}");

        return services;
    }

    private static void ValidateJwtSettings(JwtSettings jwtSettings)
    {
        if (string.IsNullOrEmpty(jwtSettings.Key) || 
            string.IsNullOrEmpty(jwtSettings.Issuer) || 
            string.IsNullOrEmpty(jwtSettings.Audience))
        {
            throw new InvalidOperationException(
                "JWT settings are incomplete. Please check your configuration and jwt-key.txt file.");
        }
    }
}