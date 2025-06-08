using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventRegistration.Application
{
    public interface IParticipantService
    {
        Task AddIndividualParticipantAsync(AddIndividualParticipantDto dto);
        Task AddCompanyParticipantAsync(AddCompanyParticipantDto dto);
        // FIX: Changed participantId from Guid to int
        Task<ParticipantViewModel?> GetParticipantDetailsAsync(int participantId, Guid eventId);
        // FIX: Changed participantId from Guid to int
        Task UpdateIndividualParticipantAsync(int participantId, Guid eventId, AddIndividualParticipantDto dto);
        // FIX: Changed participantId from Guid to int
        Task UpdateCompanyParticipantAsync(int participantId, Guid eventId, AddCompanyParticipantDto dto);
        // FIX: Changed participantId from Guid to int
        Task DeleteParticipantAsync(int participantId, Guid eventId);
        Task<IEnumerable<ParticipantViewModel>> GetParticipantsByEventAsync(Guid eventId);
        Task<IEnumerable<PaymentMethodViewModel>> GetPaymentMethodsAsync();
    }
}