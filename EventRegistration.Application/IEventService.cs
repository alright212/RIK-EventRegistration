using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventRegistration.Domain; // Assuming Event is in Domain

namespace EventRegistration.Application
{
    // Placeholder for EventViewModel - you'll need to define this
    public class EventViewModel {}

    public interface IEventService
    {
        Task<IEnumerable<EventViewModel>> GetFutureEventsAsync();
        Task<IEnumerable<EventViewModel>> GetPastEventsAsync();
        Task CreateEventAsync(CreateEventDto createEventDto);
        // ... other methods
    }
}
