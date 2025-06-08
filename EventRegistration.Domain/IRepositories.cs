using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventRegistration.Domain
{
    public interface IEventRepository
    {
        Task<Event?> GetByIdAsync(Guid id); // Changed to nullable
        Task<IEnumerable<Event>> GetAllAsync();
        Task AddAsync(Event entity);
        Task UpdateAsync(Event entity);
        Task DeleteAsync(Guid id);
    }

    public interface IParticipantRepository
    {
        Task<Participant?> GetByIdAsync(Guid id); // Changed to nullable
        Task<IEnumerable<Participant>> GetByEventIdAsync(Guid eventId); // This signature remains the same, but the implementation will change
        Task AddAsync(Participant entity);
        Task UpdateAsync(Participant entity);
        Task DeleteAsync(Guid id);
    }

    public interface IPaymentMethodRepository
    {
        Task<PaymentMethod?> GetByIdAsync(Guid id); // Changed to nullable
        Task<IEnumerable<PaymentMethod>> GetAllAsync();
        Task AddAsync(PaymentMethod entity);
        Task UpdateAsync(PaymentMethod entity);
        Task DeleteAsync(Guid id);
    }
}
