

using DataAnnotations.Data;
using Microsoft.EntityFrameworkCore;
using Repositories.Data;

public class EFCoreRepository(AppDbContext context) : IEFCoreRepository
{
    public async Task<EventRegistration?> GetEventRegistrationByIdAsync(int id)
    {
        return await context.EventRegistrations.FindAsync(id);
    }

    public async Task<(IReadOnlyCollection<EventRegistration> items, bool hasNextPage)> GetEventRegistrationsAsync(int pageSize, int lastId)
    {
        var query = context.EventRegistrations.Where(e => e.Id > lastId) .OrderBy(e => e.Id)
            .Take(pageSize + 1); // Fetch one more record to determine HasNextPage

        var result = await query.ToListAsync();

        var items = result.Take(pageSize).ToList().AsReadOnly();
        var hasNextPage = result.Count > pageSize;

        return (items, hasNextPage);
    }
}