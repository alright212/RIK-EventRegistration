using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventRegistration.Domain
{
    public interface IEventRepository
    {
        Task<Event?> GetByIdAsync(Guid id);
        Task<IEnumerable<Event>> GetAllAsync();
        Task AddAsync(Event entity);
        Task UpdateAsync(Event entity);
        Task RemoveAsync(Event entity);
        Task<Event?> GetEventWithParticipantsAsync(Guid eventId);
        Task<IEnumerable<Event>> GetUpcomingEvents();
    }

    public interface IParticipantRepository
    {
        Task<Participant?> GetByIdAsync(int id);
        Task<IEnumerable<Participant>> GetAllAsync();
        Task AddAsync(Participant entity);
        Task UpdateAsync(Participant entity);
        Task RemoveAsync(Participant entity);
        Task<IEnumerable<Participant>> GetByEventIdAsync(Guid eventId);
        Task<IndividualParticipant?> GetIndividualByPersonalIdCodeAsync(string personalIdCode);
        Task<CompanyParticipant?> GetCompanyByRegistryCodeAsync(string registryCode);
    }

    public interface IPaymentMethodRepository
    {
        Task<PaymentMethod?> GetByIdAsync(int id);
        Task<IEnumerable<PaymentMethod>> GetAllAsync();
        Task AddAsync(PaymentMethod entity);
        Task UpdateAsync(PaymentMethod entity);

        Task DeleteAsync(int id);
    }

    public interface IEventParticipantRepository
    {
        Task<IEnumerable<EventParticipant>> GetByEventIdAsync(Guid eventId);
        Task AddAsync(EventParticipant entity);
        Task UpdateAsync(EventParticipant entity);
        Task DeleteAsync(EventParticipant entity);

        Task<EventParticipant?> GetByEventAndParticipantAsync(Guid eventId, int participantId);
    }
}
