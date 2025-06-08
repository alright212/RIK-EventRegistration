using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventRegistration.Application
{
    public interface IParticipantService
    {
        Task AddIndividualParticipantAsync(AddIndividualParticipantDto dto);
        Task AddCompanyParticipantAsync(AddCompanyParticipantDto dto);
        Task<ParticipantViewModel?> GetParticipantDetailsAsync(Guid participantId, Guid eventId);
        Task UpdateIndividualParticipantAsync(Guid participantId, Guid eventId, AddIndividualParticipantDto dto); // DTO might need adjustment for update
        Task UpdateCompanyParticipantAsync(Guid participantId, Guid eventId, AddCompanyParticipantDto dto); // DTO might need adjustment for update
        Task DeleteParticipantAsync(Guid participantId, Guid eventId);
        Task<IEnumerable<ParticipantViewModel>> GetParticipantsByEventAsync(Guid eventId);
        Task<IEnumerable<PaymentMethodViewModel>> GetPaymentMethodsAsync();
    }
}
