using Dapper;
using DataAnnotations.Data;
using DataAnnotations.Middleware;
using DataAnnotations.Models;
using DataAnnotations.Services;
using events.Models;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Repositories.Data;
using DataAnnotations.Extensions;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Data;




var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod();
    });

   
});

// 1. First - Database & Identity
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("DataSource=./Data/SqliteDB.db"));

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// 2. Second - Authorization Policies (BEFORE controllers!)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("UsernameStartsWithL", policy =>
        policy.RequireAssertion(context =>
            context.User.Identity?.Name != null &&
            context.User.Identity.Name.StartsWith("L", StringComparison.OrdinalIgnoreCase)));
});


builder.Services.AddTransient<XmlFormatterMiddleware>();
// 3. Third - Controllers and other services
builder.Services.AddControllers();
builder.Services.AddControllers().AddNewtonsoftJson();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<EventRegistrationDTOValidator>();


// Auth using Cookie
// builder.Services.AddCookieAuthentication();



// Auth using Jwt
// ========================================
builder.Services.AddJwtAuthenticationWithKeyFile(
    builder.Configuration, 
    builder.Environment);

builder.Services.AddScoped<IEFCoreRepository, EFCoreRepository>();
builder.Services.AddScoped<IEFCoreService, EFCoreService>();

var connectionString = "DataSource=./Data/SqliteDB.db";
builder.Services.AddSingleton<IDapperRepository>(new DapperRepository(connectionString));
builder.Services.AddScoped<IDapperService, DapperService>();


builder.Services.AddTransient<IDbConnection>(sp => 
    new SqliteConnection(connectionString));

// For HttpOnly Middleware
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

SqlMapper.AddTypeHandler(new GuidTypeHandler());
builder.Services.AddAutoMapper(typeof(EventProfile));


// To check the health use  https://localhost:5001/api/health
builder.Services.AddHealthChecks()
    .AddCheck<DatabasePerformanceHealthCheck>("database_performance", 
        tags: ["database"]);


var app = builder.Build();

// For HttpOnly Middleware
app.UseForwardedHeaders();
app.UseMiddleware<XmlFormatterMiddleware>();
app.UseMiddleware<HttpOnlyMiddleware>();
app.UseMiddleware<AddHeadersMiddleware>();


app.UseResponseCaching();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/api/health", new HealthCheckOptions
{
    ResultStatusCodes =
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status200OK,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
    },
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description,
                duration = entry.Value.Duration
            }),
            totalDuration = report.TotalDuration
        };
        await context.Response.WriteAsJsonAsync(response);
    }
});



// DatabaseSeeder.Initialize(app.Services);
app.Run();


// Note: 
// Before using Identity for the first time, you must delete the SqlliteDB.db file and run 
// dotnet ef database update to create a new database. Otherwise, the Identity tables will not be created.