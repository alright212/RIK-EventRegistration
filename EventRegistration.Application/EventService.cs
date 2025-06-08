using EventRegistration.Domain;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventRegistration.Infrastructure;

namespace EventRegistration.Application
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;
        private readonly IParticipantRepository _participantRepository;
        private readonly EventRegistrationDbContext _dbContext;

        public EventService(IEventRepository eventRepository, IParticipantRepository participantRepository, EventRegistrationDbContext dbContext)
        {
            _eventRepository = eventRepository;
            _participantRepository = participantRepository;
            _dbContext = dbContext;
        }

        public async Task CreateEvent(CreateEventDto createEventDto)
        {
            if (createEventDto.AdditionalInfo?.Length > 1000)
            {
                throw new ArgumentException("Additional info cannot exceed 1000 characters.", nameof(createEventDto.AdditionalInfo));
            }

            var newEvent = new Event(
                createEventDto.Name,
                createEventDto.EventTime,
                createEventDto.Location,
                createEventDto.AdditionalInfo ?? string.Empty
            );

            await _eventRepository.AddAsync(newEvent);
        }

        public async Task<IEnumerable<EventViewModel>> GetUpcomingEvents()
        {
            var events = await _eventRepository.GetUpcomingEvents();
            return events
                .Select(e => new EventViewModel
                {
                    Id = e.Id,
                    Name = e.Name,
                    EventTime = e.Time,
                    Location = e.Location,
                    AdditionalInfo = e.AdditionalInfo,
                    ParticipantCount = e.Participants?.Count ?? 0
                })
                .OrderBy(e => e.EventTime);
        }

        public async Task<IEnumerable<EventViewModel>> GetPastEvents()
        {
            var events = await _eventRepository.GetAllAsync();
            return events
                .Where(e => e.Time <= DateTime.UtcNow)
                .Select(e => new EventViewModel
                {
                    Id = e.Id,
                    Name = e.Name,
                    EventTime = e.Time,
                    Location = e.Location,
                    AdditionalInfo = e.AdditionalInfo,
                    ParticipantCount = e.Participants?.Count ?? 0
                })
                .OrderByDescending(e => e.EventTime);
        }

        public async Task<EventDetailViewModel?> GetEventDetail(Guid id)
        {
            var eventEntity = await _eventRepository.GetEventWithParticipantsAsync(id);
            if (eventEntity == null)
            {
                return null;
            }

            var participants = eventEntity.Participants?.Select(ep => MapToParticipantViewModel(ep)).ToList() ?? new List<ParticipantViewModel>();

            return new EventDetailViewModel
            {
                Event = new EventViewModel
                {
                    Id = eventEntity.Id,
                    Name = eventEntity.Name,
                    EventTime = eventEntity.Time,
                    Location = eventEntity.Location,
                    AdditionalInfo = eventEntity.AdditionalInfo,
                    ParticipantCount = participants.Count
                },
                Participants = participants
            };
        }
        
        public async Task<UpdateEventDto?> GetEventForEdit(Guid id)
        {
            var eventEntity = await _eventRepository.GetByIdAsync(id);
            if (eventEntity == null)
            {
                return null;
            }
            return new UpdateEventDto
            {
                Name = eventEntity.Name,
                EventTime = eventEntity.Time,
                Location = eventEntity.Location,
                AdditionalInfo = eventEntity.AdditionalInfo
            };
        }

        public async Task UpdateEvent(Guid eventId, UpdateEventDto updateEventDto) // Changed signature
        {
            var eventToUpdate = await _eventRepository.GetByIdAsync(eventId); // Use eventId parameter
            if (eventToUpdate == null)
            {
                throw new KeyNotFoundException("Event not found.");
            }
            
            eventToUpdate.UpdateDetails(
                updateEventDto.Name,
                updateEventDto.EventTime,
                updateEventDto.Location,
                updateEventDto.AdditionalInfo
            );
            
            await _eventRepository.UpdateAsync(eventToUpdate);
        }


        public async Task DeleteEvent(Guid id)
        {
            var eventToDelete = await _eventRepository.GetByIdAsync(id);
            if (eventToDelete == null)
            {
                return; 
            }

            if (eventToDelete.Time <= DateTime.UtcNow)
            {
                throw new InvalidOperationException("Cannot delete past or current events.");
            }
            
            await _eventRepository.RemoveAsync(eventToDelete);
        }

        private ParticipantViewModel MapToParticipantViewModel(EventParticipant eventParticipant)
        {
            if (eventParticipant == null || eventParticipant.Participant == null)
            {
                throw new InvalidOperationException("EventParticipant data is incomplete for mapping.");
            }
            
            var viewModel = new ParticipantViewModel
            {
                EventId = eventParticipant.EventId,
                ParticipantId = eventParticipant.Participant.Id,
                PaymentMethodId = eventParticipant.PaymentMethodId,
                PaymentMethodName = eventParticipant.PaymentMethod?.Name ?? "Unknown",
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
