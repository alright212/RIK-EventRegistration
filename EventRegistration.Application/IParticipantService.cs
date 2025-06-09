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

        // Add method to get participant info by personal ID code for autofill
        Task<IndividualParticipantLookupDto?> GetIndividualByPersonalIdCodeAsync(
            string personalIdCode
        );

        // Add method to get company info by registry code for autofill
        Task<CompanyParticipantLookupDto?> GetCompanyByRegistryCodeAsync(string registryCode);
    }
}
