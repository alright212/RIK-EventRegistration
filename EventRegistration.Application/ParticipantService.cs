using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventRegistration.Domain;

namespace EventRegistration.Application
{
    public class ParticipantService : IParticipantService
    {
        private readonly IParticipantRepository _participantRepository;
        private readonly IEventRepository _eventRepository;
        private readonly IEventParticipantRepository _eventParticipantRepository;
        private readonly IPaymentMethodRepository _paymentMethodRepository;

        public ParticipantService(
            IParticipantRepository participantRepository,
            IEventRepository eventRepository,
            IEventParticipantRepository eventParticipantRepository,
            IPaymentMethodRepository paymentMethodRepository
        )
        {
            _participantRepository =
                participantRepository
                ?? throw new ArgumentNullException(nameof(participantRepository));
            _eventRepository =
                eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
            _eventParticipantRepository =
                eventParticipantRepository
                ?? throw new ArgumentNullException(nameof(eventParticipantRepository));
            _paymentMethodRepository =
                paymentMethodRepository
                ?? throw new ArgumentNullException(nameof(paymentMethodRepository));
        }

        public async Task AddIndividualParticipantAsync(AddIndividualParticipantDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var eventExists = await _eventRepository.GetByIdAsync(dto.EventId);
            if (eventExists == null)
                throw new ArgumentException("Event not found.", nameof(dto.EventId));

            var paymentMethodExists = await _paymentMethodRepository.GetByIdAsync(
                dto.PaymentMethodId
            );
            if (paymentMethodExists == null)
                throw new ArgumentException(
                    "Payment method not found.",
                    nameof(dto.PaymentMethodId)
                );

            var participant = await _participantRepository.GetIndividualByPersonalIdCodeAsync(
                dto.PersonalIdCode
            );
            if (participant == null)
            {
                participant = new IndividualParticipant(
                    dto.FirstName,
                    dto.LastName,
                    dto.PersonalIdCode
                );
                await _participantRepository.AddAsync(participant);
            }
            else
            {
                // Update the participant's name if it has changed
                if (participant.FirstName != dto.FirstName || participant.LastName != dto.LastName)
                {
                    participant.FirstName = dto.FirstName;
                    participant.LastName = dto.LastName;
                    await _participantRepository.UpdateAsync(participant);
                }
            }

            // The participant.Id is now an int, matching the repository method signature.
            var existingRegistration =
                await _eventParticipantRepository.GetByEventAndParticipantAsync(
                    dto.EventId,
                    participant.Id
                );
            if (existingRegistration != null)
            {
                throw new InvalidOperationException(
                    "Participant is already registered for this event."
                );
            }

            var eventParticipant = new EventParticipant
            {
                EventId = dto.EventId,
                ParticipantId = participant.Id,
                PaymentMethodId = dto.PaymentMethodId,
                AdditionalInfo = dto.AdditionalInfo,
            };
            await _eventParticipantRepository.AddAsync(eventParticipant);
        }

        public async Task AddCompanyParticipantAsync(AddCompanyParticipantDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var eventExists = await _eventRepository.GetByIdAsync(dto.EventId);
            if (eventExists == null)
                throw new ArgumentException("Event not found.", nameof(dto.EventId));

            var paymentMethodExists = await _paymentMethodRepository.GetByIdAsync(
                dto.PaymentMethodId
            );
            if (paymentMethodExists == null)
                throw new ArgumentException(
                    "Payment method not found.",
                    nameof(dto.PaymentMethodId)
                );

            var participant = await _participantRepository.GetCompanyByRegistryCodeAsync(
                dto.RegistryCode
            );
            if (participant == null)
            {
                participant = new CompanyParticipant(
                    dto.LegalName,
                    dto.RegistryCode
                );
                await _participantRepository.AddAsync(participant);
            }
            else
            {
                // Update the participant's information if it has changed
                if (participant.LegalName != dto.LegalName)
                {
                    participant.LegalName = dto.LegalName;
                    await _participantRepository.UpdateAsync(participant);
                }
            }

            // The participant.Id is now an int, matching the repository method signature.
            var existingRegistration =
                await _eventParticipantRepository.GetByEventAndParticipantAsync(
                    dto.EventId,
                    participant.Id
                );
            if (existingRegistration != null)
            {
                throw new InvalidOperationException(
                    "Participant is already registered for this event."
                );
            }

            var eventParticipant = new EventParticipant
            {
                EventId = dto.EventId,
                ParticipantId = participant.Id,
                PaymentMethodId = dto.PaymentMethodId,
                AdditionalInfo = dto.AdditionalInfo,
                NumberOfParticipants = dto.NumberOfParticipants
            };
            await _eventParticipantRepository.AddAsync(eventParticipant);
        }

        // FIX: Changed participantId parameter from Guid to int
        public async Task<ParticipantViewModel?> GetParticipantDetailsAsync(
            int participantId,
            Guid eventId
        )
        {
            var eventParticipant = await _eventParticipantRepository.GetByEventAndParticipantAsync(
                eventId,
                participantId
            );

            if (
                eventParticipant == null
                || eventParticipant.Participant == null
                || eventParticipant.PaymentMethod == null
            )
            {
                return null;
            }

            return MapToParticipantViewModel(eventParticipant);
        }

        // FIX: Changed participantId parameter from Guid to int
        public async Task UpdateIndividualParticipantAsync(
            int participantId,
            Guid eventId,
            AddIndividualParticipantDto dto
        )
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var eventParticipant = await _eventParticipantRepository.GetByEventAndParticipantAsync(
                eventId,
                participantId
            );
            if (eventParticipant == null || eventParticipant.Participant == null)
            {
                throw new ArgumentException("Participant registration not found.");
            }

            if (!(eventParticipant.Participant is IndividualParticipant individualParticipant))
            {
                throw new InvalidOperationException("Participant is not an individual.");
            }

            var paymentMethodExists = await _paymentMethodRepository.GetByIdAsync(
                dto.PaymentMethodId
            );
            if (paymentMethodExists == null)
                throw new ArgumentException(
                    "Payment method not found.",
                    nameof(dto.PaymentMethodId)
                );

            individualParticipant.FirstName = dto.FirstName;
            individualParticipant.LastName = dto.LastName;
            individualParticipant.PersonalIdCode = dto.PersonalIdCode;
            await _participantRepository.UpdateAsync(individualParticipant);

            eventParticipant.PaymentMethodId = dto.PaymentMethodId;
            eventParticipant.AdditionalInfo = dto.AdditionalInfo;
            await _eventParticipantRepository.UpdateAsync(eventParticipant);
        }

        // FIX: Changed participantId parameter from Guid to int
        public async Task UpdateCompanyParticipantAsync(
            int participantId,
            Guid eventId,
            AddCompanyParticipantDto dto
        )
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var eventParticipant = await _eventParticipantRepository.GetByEventAndParticipantAsync(
                eventId,
                participantId
            );
            if (eventParticipant == null || eventParticipant.Participant == null)
            {
                throw new ArgumentException("Participant registration not found.");
            }

            if (!(eventParticipant.Participant is CompanyParticipant companyParticipant))
            {
                throw new InvalidOperationException("Participant is not a company.");
            }

            var paymentMethodExists = await _paymentMethodRepository.GetByIdAsync(
                dto.PaymentMethodId
            );
            if (paymentMethodExists == null)
                throw new ArgumentException(
                    "Payment method not found.",
                    nameof(dto.PaymentMethodId)
                );

            companyParticipant.LegalName = dto.LegalName;
            companyParticipant.RegistryCode = dto.RegistryCode;
            await _participantRepository.UpdateAsync(companyParticipant);

            eventParticipant.PaymentMethodId = dto.PaymentMethodId;
            eventParticipant.AdditionalInfo = dto.AdditionalInfo;
            eventParticipant.NumberOfParticipants = dto.NumberOfParticipants;
            await _eventParticipantRepository.UpdateAsync(eventParticipant);
        }

        // FIX: Changed participantId parameter from Guid to int
        public async Task DeleteParticipantAsync(int participantId, Guid eventId)
        {
            var eventParticipant = await _eventParticipantRepository.GetByEventAndParticipantAsync(
                eventId,
                participantId
            );
            if (eventParticipant == null)
            {
                throw new ArgumentException("Participant registration not found.");
            }

            await _eventParticipantRepository.DeleteAsync(eventParticipant);
        }

        public async Task<IEnumerable<ParticipantViewModel>> GetParticipantsByEventAsync(
            Guid eventId
        )
        {
            var eventParticipants = await _eventParticipantRepository.GetByEventIdAsync(eventId);
            return eventParticipants.Select(MapToParticipantViewModel).ToList();
        }

        public async Task<IEnumerable<PaymentMethodViewModel>> GetPaymentMethodsAsync()
        {
            var paymentMethods = await _paymentMethodRepository.GetAllAsync();
            // Note: Your PaymentMethodViewModel still uses Guid for the Id.
            // This should be changed to int for consistency.
            // Assuming it is changed to int:
            return paymentMethods
                .Select(pm => new PaymentMethodViewModel
                {
                    Id = pm.Id, // pm.Id is an 'int', but PaymentMethodViewModel.Id expects a 'Guid'
                    Name = pm.Name,
                })
                .ToList();
        }

        public async Task<IndividualParticipantLookupDto?> GetIndividualByPersonalIdCodeAsync(
            string personalIdCode
        )
        {
            if (string.IsNullOrWhiteSpace(personalIdCode))
                return null;

            var participant = await _participantRepository.GetIndividualByPersonalIdCodeAsync(
                personalIdCode
            );
            if (participant == null)
                return null;

            return new IndividualParticipantLookupDto
            {
                FirstName = participant.FirstName,
                LastName = participant.LastName,
                PersonalIdCode = participant.PersonalIdCode,
            };
        }

        public async Task<CompanyParticipantLookupDto?> GetCompanyByRegistryCodeAsync(
            string registryCode
        )
        {
            if (string.IsNullOrWhiteSpace(registryCode))
                return null;

            var participant = await _participantRepository.GetCompanyByRegistryCodeAsync(
                registryCode
            );
            if (participant == null)
                return null;

            return new CompanyParticipantLookupDto
            {
                LegalName = participant.LegalName,
                RegistryCode = participant.RegistryCode,
            };
        }

        private ParticipantViewModel MapToParticipantViewModel(EventParticipant eventParticipant)
        {
            if (
                eventParticipant == null
                || eventParticipant.Participant == null
                || eventParticipant.PaymentMethod == null
            )
            {
                throw new InvalidOperationException(
                    "EventParticipant data is incomplete for mapping."
                );
            }

            var viewModel = new ParticipantViewModel
            {
                EventId = eventParticipant.EventId,
                ParticipantId = eventParticipant.Participant.Id,
                PaymentMethodId = eventParticipant.PaymentMethodId,
                PaymentMethodName = eventParticipant.PaymentMethod.Name,
                EventParticipantAdditionalInfo = eventParticipant.AdditionalInfo,
            };

            if (eventParticipant.Participant is IndividualParticipant individual)
            {
                viewModel.ParticipantType = "Individual";
                viewModel.FirstName = individual.FirstName;
                viewModel.LastName = individual.LastName;
                viewModel.PersonalIdCode = individual.PersonalIdCode;
            }
            else if (eventParticipant.Participant is CompanyParticipant company)
            {
                viewModel.ParticipantType = "Company";
                viewModel.LegalName = company.LegalName;
                viewModel.RegistryCode = company.RegistryCode;
                viewModel.NumberOfParticipants = eventParticipant.NumberOfParticipants ?? 0;
            }

            return viewModel;
        }
    }
}
