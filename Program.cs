using Bogus;
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
using Microsoft.AspNetCore.Authentication.Cookies;
using DataAnnotations.Extensions;



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


// For HttpOnly Middleware
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

SqlMapper.AddTypeHandler(new GuidTypeHandler());
builder.Services.AddAutoMapper(typeof(EventProfile));

var app = builder.Build();

// For HttpOnly Middleware
app.UseForwardedHeaders();
app.UseMiddleware<HttpOnlyMiddleware>();


app.UseResponseCaching();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

var connectionStringBuilder = new SqliteConnectionStringBuilder();
connectionStringBuilder.DataSource  = "./Data/SqliteDB.db";




// using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
// {
//     connection.Open();

//     var createTableCommand = connection.CreateCommand();

//     createTableCommand.CommandText = @"
//         CREATE TABLE IF NOT EXISTS EventRegistrations (
//             Id INTEGER PRIMARY KEY AUTOINCREMENT,
//             GUID TEXT,
//             FullName TEXT,
//             Email TEXT,
//             EventName TEXT,
//             EventDate TEXT,
//             DaysAttending INTEGER,
//             Notes TEXT,
//             PhoneNumber TEXT,
//             Address TEXT
//         )
//     ";

//     createTableCommand.ExecuteNonQuery();

//     var checkTableCommand = connection.CreateCommand();
//     checkTableCommand.CommandText = "SELECT COUNT(*) FROM EventRegistrations";

//     var count = Convert.ToInt64(checkTableCommand.ExecuteScalar());

//     if(count == 0)
//     {

//         var additionalContactFaker = new Faker<AdditionalContactInfo>()
//             .RuleFor(ac => ac.PhoneNumber, f => f.Phone.PhoneNumber())
//             .RuleFor(ac => ac.Address, f => f.Address.FullAddress());


//         var faker = new Faker<EventRegistration>()
//             .RuleFor(e => e.GUID, f => Guid.NewGuid())
//             .RuleFor(e => e.FullName, f => f.Name.FullName())
//             .RuleFor(e => e.Email, f => f.Internet.Email())
//             .RuleFor(e => e.EventName, f => f.Lorem.Word())
//             .RuleFor(e => e.EventDate, f => f.Date.Future())    
//             .RuleFor(e => e.DaysAttending, f => f.Random.Int(1, 7))
//             .RuleFor(e => e.Notes, f => f.Lorem.Sentence())
//             .RuleFor(e => e.AdditionalContact, f => additionalContactFaker.Generate());


//         var registrations = faker.Generate(10000);

//         using (var transaction = connection.BeginTransaction())
//         {
//             var insertCommand = connection.CreateCommand();
//             insertCommand.CommandText = @"  
//                 INSERT INTO EventRegistrations (GUID, FullName, Email, EventName, EventDate, DaysAttending, Notes, PhoneNumber, Address)
//                 VALUES (@GUID, @FullName, @Email, @EventName, @EventDate, @DaysAttending, @Notes, @PhoneNumber, @Address)
//             ";

//             insertCommand.Transaction = transaction;
//             foreach (var registration in registrations)
//             {
//                 insertCommand.Parameters.Clear();
//                 insertCommand.Parameters.AddWithValue("@GUID", registration.GUID);
//                 insertCommand.Parameters.AddWithValue("@FullName", registration.FullName);
//                 insertCommand.Parameters.AddWithValue("@Email", registration.Email);
//                 insertCommand.Parameters.AddWithValue("@EventName", registration.EventName);
//                 insertCommand.Parameters.AddWithValue("@EventDate", registration.EventDate.ToString("yyyy-MM-dd"));
//                 insertCommand.Parameters.AddWithValue("@DaysAttending", registration.DaysAttending);
//                 insertCommand.Parameters.AddWithValue("@Notes", registration.Notes);
//                 insertCommand.Parameters.AddWithValue("@PhoneNumber", registration.AdditionalContact.PhoneNumber);
//                 insertCommand.Parameters.AddWithValue("@Address", registration.AdditionalContact.Address);
//                 insertCommand.ExecuteNonQuery();

//             }

//             transaction.Commit();
//         }
//     }
// }


app.Run();


// Note: 
// Before using Identity for the first time, you must delete the SqlliteDB.db file and run 
// dotnet ef database update to create a new database. Otherwise, the Identity tables will not be created.