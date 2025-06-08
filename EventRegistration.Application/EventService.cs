using EventRegistration.Domain;
using System.Linq;

namespace EventRegistration.Application
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;

        public EventService(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public async Task CreateEventAsync(CreateEventDto createEventDto)
        {
            // Validation for AdditionalInfo length, EventTime is validated in Event constructor
            if (createEventDto.AdditionalInfo?.Length > 1000)
            {
                 throw new ArgumentException("Additional info cannot exceed 1000 characters.", nameof(createEventDto.AdditionalInfo));
            }

            var newEvent = new Event(
                createEventDto.Name,
                createEventDto.EventTime,
                createEventDto.Location,
                createEventDto.AdditionalInfo ?? string.Empty // Ensure AdditionalInfo is not null for the constructor
            );

            await _eventRepository.AddAsync(newEvent);
        }

        public async Task<IEnumerable<EventViewModel>> GetFutureEventsAsync()
        {
            var events = await _eventRepository.GetAllAsync(); 
            return events
                .Where(e => e.EventTime > DateTime.UtcNow)
                .Select(e => new EventViewModel
                {
                    Id = e.Id,
                    Name = e.Name,
                    EventTime = e.EventTime,
                    Location = e.Location,
                    AdditionalInfo = e.AdditionalInfo,
                    ParticipantCount = e.EventParticipants?.Count ?? 0 
                })
                .OrderBy(e => e.EventTime);
        }

        public async Task<IEnumerable<EventViewModel>> GetPastEventsAsync()
        {
            var events = await _eventRepository.GetAllAsync();
            return events
                .Where(e => e.EventTime <= DateTime.UtcNow)
                .Select(e => new EventViewModel
                {
                    Id = e.Id,
                    Name = e.Name,
                    EventTime = e.EventTime,
                    Location = e.Location,
                    AdditionalInfo = e.AdditionalInfo,
                    ParticipantCount = e.EventParticipants?.Count ?? 0 
                })
                .OrderByDescending(e => e.EventTime);
        }

        public async Task<EventViewModel?> GetEventDetailsAsync(Guid eventId)
        {
            var eventEntity = await _eventRepository.GetByIdAsync(eventId);
            if (eventEntity == null)
            {
                return null;
            }

            return new EventViewModel
            {
                Id = eventEntity.Id,
                Name = eventEntity.Name,
                EventTime = eventEntity.EventTime,
                Location = eventEntity.Location,
                AdditionalInfo = eventEntity.AdditionalInfo,
                ParticipantCount = eventEntity.EventParticipants?.Count ?? 0 
            };
        }
    }
}
