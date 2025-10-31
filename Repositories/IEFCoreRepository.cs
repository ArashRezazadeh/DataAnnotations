


using DataAnnotations.Data;

namespace Repositories.Data;

public interface IEFCoreRepository
{
    Task<(IReadOnlyCollection<EventRegistration> items, bool hasNextPage)> GetEventRegistrationsAsync(int pageSize, int lastId);
    Task<EventRegistration?> GetEventRegistrationByIdAsync(int id);
} 


