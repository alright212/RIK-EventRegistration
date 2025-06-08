using EventRegistration.Domain;
using EventRegistration.Application;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace EventRegistration.Application
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;
        // Assuming IParticipantRepository and IPaymentMethodRepository will be needed later
        // private readonly IParticipantRepository _participantRepository;
        // private readonly IPaymentMethodRepository _paymentMethodRepository;

        public EventService(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public async Task CreateEventAsync(CreateEventDto createEventDto)
        {
            var newEvent = new Event(
                createEventDto.Name,
                createEventDto.EventTime,
                createEventDto.Location,
                createEventDto.AdditionalInfo
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
                    // Populate EventViewModel properties here
                    // Name = e.Name, EventTime = e.EventTime, etc.
                });
        }

        public async Task<IEnumerable<EventViewModel>> GetPastEventsAsync()
        {
            var events = await _eventRepository.GetAllAsync();
            return events
                .Where(e => e.EventTime <= DateTime.UtcNow)
                .Select(e => new EventViewModel 
                { 
                    // Populate EventViewModel properties here
                    // Name = e.Name, EventTime = e.EventTime, etc.
                });
        }

        // ... other implementations
    }
}
