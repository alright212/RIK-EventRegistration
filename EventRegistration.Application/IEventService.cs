using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventRegistration.Application
{
    public interface IEventService
    {
        Task<IEnumerable<EventViewModel>> GetFutureEventsAsync();
        Task<IEnumerable<EventViewModel>> GetPastEventsAsync();
        Task CreateEventAsync(CreateEventDto createEventDto);
        Task<EventViewModel?> GetEventDetailsAsync(Guid eventId);
        // ... other methods
    }
}
