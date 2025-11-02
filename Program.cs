using Bogus;
using Dapper;
using DataAnnotations.Data;
using DataAnnotations.Models;
using DataAnnotations.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Repositories.Data;

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

builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<EventRegistrationDTOValidator>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("DataSource=./Data/SqliteDB.db" ));

builder.Services.AddScoped<IEFCoreRepository, EFCoreRepository>();
builder.Services.AddScoped<IEFCoreService, EFCoreService>();

var connectionString = "DataSource=./Data/SqliteDB.db";
builder.Services.AddSingleton<IDapperRepository>(new DapperRepository(connectionString));
builder.Services.AddScoped<IDapperService, DapperService>();

SqlMapper.AddTypeHandler(new GuidTypeHandler());


var app = builder.Build();
app.UseResponseCaching();
app.MapControllers();

var connectionStringBuilder = new SqliteConnectionStringBuilder();
connectionStringBuilder.DataSource  = "./Data/SqliteDB.db";

using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
{
    connection.Open();

    var createTableCommand = connection.CreateCommand();

    createTableCommand.CommandText = @"
        CREATE TABLE IF NOT EXISTS EventRegistrations (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            GUID TEXT,
            FullName TEXT,
            Email TEXT,
            EventName TEXT,
            EventDate TEXT,
            DaysAttending INTEGER,
            Notes TEXT
        )
    ";

    createTableCommand.ExecuteNonQuery();

    var checkTableCommand = connection.CreateCommand();
    checkTableCommand.CommandText = "SELECT COUNT(*) FROM EventRegistrations";

    var count = Convert.ToInt64(checkTableCommand.ExecuteScalar());

    if(count == 0)
    {
        var faker = new Faker<EventRegistration>()
            .RuleFor(e => e.GUID, f => Guid.NewGuid())
            .RuleFor(e => e.FullName, f => f.Name.FullName())
            .RuleFor(e => e.Email, f => f.Internet.Email())
            .RuleFor(e => e.EventName, f => f.Lorem.Word())
            .RuleFor(e => e.EventDate, f => f.Date.Future())
            .RuleFor(e => e.DaysAttending, f => f.Random.Int(1, 7))
            .RuleFor(e => e.Notes, f => f.Lorem.Sentence());

        var registrations = faker.Generate(10000);

        using (var transaction = connection.BeginTransaction())
        {
            var insertCommand = connection.CreateCommand();
            insertCommand.CommandText = @"
                INSERT INTO EventRegistrations (GUID, FullName, Email, EventName, EventDate, DaysAttending, Notes)
                VALUES (@GUID, @FullName, @Email, @EventName, @EventDate, @DaysAttending, @Notes)";

            insertCommand.Transaction = transaction;
            foreach (var registration in registrations)
            {
                insertCommand.Parameters.Clear();
                insertCommand.Parameters.AddWithValue("@GUID", registration.GUID);
                insertCommand.Parameters.AddWithValue("@FullName", registration.FullName);
                insertCommand.Parameters.AddWithValue("@Email", registration.Email);
                insertCommand.Parameters.AddWithValue("@EventName", registration.EventName);
                insertCommand.Parameters.AddWithValue("@EventDate", registration.EventDate.ToString("yyyy-MM-dd"));
                insertCommand.Parameters.AddWithValue("@DaysAttending", registration.DaysAttending);
                insertCommand.Parameters.AddWithValue("@Notes", registration.Notes);
                insertCommand.ExecuteNonQuery();

            }

            transaction.Commit();
        }
    }
}


app.Run();

