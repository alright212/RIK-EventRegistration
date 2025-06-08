using EventRegistration.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            IPaymentMethodRepository paymentMethodRepository)
        {
            _participantRepository = participantRepository ?? throw new ArgumentNullException(nameof(participantRepository));
            _eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
            _eventParticipantRepository = eventParticipantRepository ?? throw new ArgumentNullException(nameof(eventParticipantRepository));
            _paymentMethodRepository = paymentMethodRepository ?? throw new ArgumentNullException(nameof(paymentMethodRepository));
        }

        public async Task AddIndividualParticipantAsync(AddIndividualParticipantDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var eventExists = await _eventRepository.GetByIdAsync(dto.EventId);
            if (eventExists == null) throw new ArgumentException("Event not found.", nameof(dto.EventId));

            var paymentMethodExists = await _paymentMethodRepository.GetByIdAsync(dto.PaymentMethodId);
            if (paymentMethodExists == null) throw new ArgumentException("Payment method not found.", nameof(dto.PaymentMethodId));

            
            var participant = await _participantRepository.GetIndividualByPersonalIdCodeAsync(dto.PersonalIdCode);
            if (participant == null)
            {
                participant = new IndividualParticipant(dto.FirstName, dto.LastName, dto.PersonalIdCode);
                await _participantRepository.AddAsync(participant);
            }

            
            var existingRegistration = await _eventParticipantRepository.GetByEventAndParticipantAsync(dto.EventId, participant.Id);
            if (existingRegistration != null)
            {
                throw new InvalidOperationException("Participant is already registered for this event.");
            }

            var eventParticipant = new EventParticipant
            {
                
                EventId = dto.EventId,
                ParticipantId = participant.Id,
                PaymentMethodId = dto.PaymentMethodId,
                AdditionalInfo = dto.AdditionalInfo
            };
            await _eventParticipantRepository.AddAsync(eventParticipant);
        }

        public async Task AddCompanyParticipantAsync(AddCompanyParticipantDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var eventExists = await _eventRepository.GetByIdAsync(dto.EventId);
            if (eventExists == null) throw new ArgumentException("Event not found.", nameof(dto.EventId));
            
            var paymentMethodExists = await _paymentMethodRepository.GetByIdAsync(dto.PaymentMethodId);
            if (paymentMethodExists == null) throw new ArgumentException("Payment method not found.", nameof(dto.PaymentMethodId));

            
            var participant = await _participantRepository.GetCompanyByRegistryCodeAsync(dto.RegistryCode);
            if (participant == null)
            {
                participant = new CompanyParticipant(dto.LegalName, dto.RegistryCode, dto.NumberOfParticipants);
                await _participantRepository.AddAsync(participant);
            }
            else
            {
                // Check if the number of participants has changed and update if necessary
                if (participant.NumberOfParticipants != dto.NumberOfParticipants)
                {
                    participant.NumberOfParticipants = dto.NumberOfParticipants;
                    await _participantRepository.UpdateAsync(participant); // Update the participant in the repository
                }
            }

            var existingRegistration = await _eventParticipantRepository.GetByEventAndParticipantAsync(dto.EventId, participant.Id);
            if (existingRegistration != null)
            {
                throw new InvalidOperationException("Participant is already registered for this event.");
            }

            var eventParticipant = new EventParticipant
            {
                EventId = dto.EventId,
                ParticipantId = participant.Id,
                PaymentMethodId = dto.PaymentMethodId,
                AdditionalInfo = dto.AdditionalInfo
                
                
            };
            await _eventParticipantRepository.AddAsync(eventParticipant);
        }

        public async Task<ParticipantViewModel?> GetParticipantDetailsAsync(Guid participantId, Guid eventId)
        {
            
            
            var eventParticipant = await _eventParticipantRepository.GetByEventAndParticipantAsync(eventId, participantId);

            if (eventParticipant == null || eventParticipant.Participant == null || eventParticipant.PaymentMethod == null)
            {
                return null;
            }

            
            return MapToParticipantViewModel(eventParticipant);
        }

        public async Task UpdateIndividualParticipantAsync(Guid participantId, Guid eventId, AddIndividualParticipantDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var eventParticipant = await _eventParticipantRepository.GetByEventAndParticipantAsync(eventId, participantId);
            if (eventParticipant == null || eventParticipant.Participant == null)
            {
                throw new ArgumentException("Participant registration not found.");
            }

            if (!(eventParticipant.Participant is IndividualParticipant individualParticipant))
            {
                throw new InvalidOperationException("Participant is not an individual.");
            }

            var paymentMethodExists = await _paymentMethodRepository.GetByIdAsync(dto.PaymentMethodId);
            if (paymentMethodExists == null) throw new ArgumentException("Payment method not found.", nameof(dto.PaymentMethodId));

            
            
            individualParticipant.FirstName = dto.FirstName;
            individualParticipant.LastName = dto.LastName;
            individualParticipant.PersonalIdCode = dto.PersonalIdCode;
            await _participantRepository.UpdateAsync(individualParticipant);

            
            eventParticipant.PaymentMethodId = dto.PaymentMethodId;
            eventParticipant.AdditionalInfo = dto.AdditionalInfo;
            await _eventParticipantRepository.UpdateAsync(eventParticipant);
        }

        public async Task UpdateCompanyParticipantAsync(Guid participantId, Guid eventId, AddCompanyParticipantDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var eventParticipant = await _eventParticipantRepository.GetByEventAndParticipantAsync(eventId, participantId);
            if (eventParticipant == null || eventParticipant.Participant == null)
            {
                throw new ArgumentException("Participant registration not found.");
            }

            if (!(eventParticipant.Participant is CompanyParticipant companyParticipant))
            {
                throw new InvalidOperationException("Participant is not a company.");
            }
            
            var paymentMethodExists = await _paymentMethodRepository.GetByIdAsync(dto.PaymentMethodId);
            if (paymentMethodExists == null) throw new ArgumentException("Payment method not found.", nameof(dto.PaymentMethodId));

            
            companyParticipant.LegalName = dto.LegalName;
            companyParticipant.RegistryCode = dto.RegistryCode;
            companyParticipant.NumberOfParticipants = dto.NumberOfParticipants; 
            await _participantRepository.UpdateAsync(companyParticipant);

            
            eventParticipant.PaymentMethodId = dto.PaymentMethodId;
            eventParticipant.AdditionalInfo = dto.AdditionalInfo;
            await _eventParticipantRepository.UpdateAsync(eventParticipant);
        }

        public async Task DeleteParticipantAsync(Guid participantId, Guid eventId)
        {
            var eventParticipant = await _eventParticipantRepository.GetByEventAndParticipantAsync(eventId, participantId);
            if (eventParticipant == null)
            {
                throw new ArgumentException("Participant registration not found.");
            }

            // Use the composite key (EventId, ParticipantId) to delete the record
            // The DeleteAsync method in IEventParticipantRepository needs to be able to handle this.
            // Assuming IEventParticipantRepository.DeleteAsync is updated to take eventId and participantId or to fetch the entity by these and then delete.
            // For now, let's assume there's a way to delete it, perhaps by fetching it first if DeleteAsync still expects an Id or a full entity.
            // If _eventParticipantRepository.DeleteAsync expects the entity, we already have it in 'eventParticipant'.
            // If it expects an Id, and since we removed the single 'Id' property, this part needs to be reconciled with IEventParticipantRepository's DeleteAsync signature.
            // For the purpose of this fix, and assuming DeleteAsync can take the entity:
            await _eventParticipantRepository.DeleteAsync(eventParticipant); // This line assumes DeleteAsync can accept the entity or is adapted.
                                                                      // If DeleteAsync strictly requires a Guid Id, this will need further adjustment in IEventParticipantRepository and its implementation.
        }

        public async Task<IEnumerable<ParticipantViewModel>> GetParticipantsByEventAsync(Guid eventId)
        {
            
            var eventParticipants = await _eventParticipantRepository.GetByEventIdAsync(eventId);
            return eventParticipants.Select(ep => MapToParticipantViewModel(ep)).ToList();
        }

        public async Task<IEnumerable<PaymentMethodViewModel>> GetPaymentMethodsAsync()
        {
            var paymentMethods = await _paymentMethodRepository.GetAllAsync();
            return paymentMethods.Select(pm => new PaymentMethodViewModel
            {
                Id = pm.Id,
                Name = pm.Name
            }).ToList();
        }

        
        private ParticipantViewModel MapToParticipantViewModel(EventParticipant eventParticipant)
        {
            if (eventParticipant == null || eventParticipant.Participant == null || eventParticipant.PaymentMethod == null)
            {
                throw new InvalidOperationException("EventParticipant data is incomplete for mapping.");
            }
            
            var viewModel = new ParticipantViewModel
            {
                // EventParticipantId is no longer a single GUID. We use EventId and ParticipantId instead.
                EventId = eventParticipant.EventId,
                ParticipantId = eventParticipant.Participant.Id,
                PaymentMethodId = eventParticipant.PaymentMethodId,
                PaymentMethodName = eventParticipant.PaymentMethod.Name,
                EventParticipantAdditionalInfo = eventParticipant.AdditionalInfo
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
                viewModel.NumberOfParticipants = company.NumberOfParticipants;
            }
            

            return viewModel;
        }
    }
}
