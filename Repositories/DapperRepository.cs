using Dapper;
using Microsoft.Data.Sqlite;
using Repositories.Data;

namespace DataAnnotations.Data;


public class DapperRepository(string connectionString) : IDapperRepository
{
    public async Task<EventRegistration> CreateEventRegistrationAsync(EventRegistration eventRegistration)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            await connection.OpenAsync();

            string sql = @"
                INSERT INTO EventRegistrations (GUID, FullName, Email, EventName, EventDate, DaysAttending, Notes)
                VALUES (@GUID, @FullName, @Email, @EventName, @EventDate, @DaysAttending, @Notes);
                SELECT last_insert_rowid();";

            var parameters = new
            {
                GUID = eventRegistration.GUID,
                FullName = eventRegistration.FullName,
                Email = eventRegistration.Email,
                EventName = eventRegistration.EventName,
                EventDate = eventRegistration.EventDate,
                DaysAttending = eventRegistration.DaysAttending,
                Notes = eventRegistration.Notes
            };

            var id = await connection.QuerySingleAsync<int>(sql, parameters);

            eventRegistration.Id = id;

            return eventRegistration;
        }
    }

    public async Task<EventRegistration?> GetEventRegistrationByIdAsync(int id)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            await connection.OpenAsync();

            string sql =
                @"SELECT Id, GUID, FullName, Email, 
                EventName, EventDate, DaysAttending, Notes 
                FROM EventRegistrations 
                WHERE Id = @Id";
                
            var parameters = new { Id = id };
            return await connection.QueryFirstOrDefaultAsync<EventRegistration>(sql, parameters);
        }
    }

    public async Task<(IReadOnlyCollection<EventRegistration> items, bool hasNextPage)> GetEventRegistrationsAsync(int pageSize, int lastId)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            await connection.OpenAsync();

            string sql = @"
                SELECT Id, GUID, FullName, Email,
                EventName, EventDate, DaysAttending, Notes 
                FROM EventRegistrations
                WHERE Id > @LastId
                ORDER BY Id
                LIMIT @PageSize + 1";  // Fetch one more record to determine HasNextPage

            var parameters = new { LastId = lastId, PageSize = pageSize };

            var result = await connection.QueryAsync<EventRegistration>(sql, parameters);

            var items = result.Take(pageSize).ToList().AsReadOnly();
            var hasNextPage = result.Count() > pageSize;


            return (items, hasNextPage);
        }
    }

    public async Task UpdateEventRegistrationAsync(EventRegistration eventRegistration)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            var query = @"
                UPDATE EventRegistrations
                SET FullName = @FullName,
                    Email = @Email,
                    EventName = @EventName,
                    EventDate = @EventDate,
                    DaysAttending = @DaysAttending,
                    Notes = @Notes,
                    PhoneNumber = @PhoneNumber,
                    Address = @Address
                WHERE Id = @Id";

            await connection.ExecuteAsync(query, new
            {
                eventRegistration.FullName,
                eventRegistration.Email,
                eventRegistration.EventName,
                eventRegistration.EventDate,
                eventRegistration.DaysAttending,
                eventRegistration.Notes,
                eventRegistration.AdditionalContact.PhoneNumber,
                eventRegistration.AdditionalContact.Address,
                eventRegistration.Id
            });
        }
    }

     public async Task DeleteEventRegistrationAsync(int id)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            var query = @"
                DELETE FROM EventRegistrations 
                WHERE Id = @Id";
            await connection.ExecuteAsync(query, new { Id = id });
        }
    }
}