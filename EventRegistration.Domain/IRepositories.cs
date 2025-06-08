using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventRegistration.Domain
{
    public interface IEventRepository
    {
        Task<Event> GetByIdAsync(Guid id);
        Task<IEnumerable<Event>> GetAllAsync();
        Task AddAsync(Event entity);
        Task UpdateAsync(Event entity);
        Task DeleteAsync(Guid id);
    }

    public interface IParticipantRepository
    {
        Task<Participant> GetByIdAsync(Guid id);
        Task<IEnumerable<Participant>> GetByEventIdAsync(Guid eventId);
        Task AddAsync(Participant entity);
        Task UpdateAsync(Participant entity);
        Task DeleteAsync(Guid id);
    }

    public interface IPaymentMethodRepository
    {
        Task<PaymentMethod> GetByIdAsync(Guid id);
        Task<IEnumerable<PaymentMethod>> GetAllAsync();
        Task AddAsync(PaymentMethod entity);
        Task UpdateAsync(PaymentMethod entity);
        Task DeleteAsync(Guid id);
    }
}
