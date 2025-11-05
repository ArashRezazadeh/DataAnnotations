using DataAnnotations.Data;

namespace Repositories.Data;


public interface IDapperRepository
{
    Task<(IReadOnlyCollection<EventRegistration> items, bool hasNextPage)> GetEventRegistrationsAsync(int pageSize, int lastId);
    Task<EventRegistration?> GetEventRegistrationByIdAsync(int id);
    Task<EventRegistration> CreateEventRegistrationAsync(EventRegistration eventRegistration);
    Task UpdateEventRegistrationAsync(EventRegistration eventRegistration);
    Task DeleteEventRegistrationAsync(int id);
}