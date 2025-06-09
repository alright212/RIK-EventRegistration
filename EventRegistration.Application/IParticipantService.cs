using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventRegistration.Application
{
    public interface IParticipantService
    {
        Task AddIndividualParticipantAsync(AddIndividualParticipantDto dto);
        Task AddCompanyParticipantAsync(AddCompanyParticipantDto dto);

        Task<ParticipantViewModel?> GetParticipantDetailsAsync(int participantId, Guid eventId);

        Task UpdateIndividualParticipantAsync(
            int participantId,
            Guid eventId,
            AddIndividualParticipantDto dto
        );

        Task UpdateCompanyParticipantAsync(
            int participantId,
            Guid eventId,
            AddCompanyParticipantDto dto
        );

        Task DeleteParticipantAsync(int participantId, Guid eventId);
        Task<IEnumerable<ParticipantViewModel>> GetParticipantsByEventAsync(Guid eventId);
        Task<IEnumerable<PaymentMethodViewModel>> GetPaymentMethodsAsync();
    }
}
