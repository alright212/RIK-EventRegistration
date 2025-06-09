using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventRegistration.Application
{
    public interface IEventService
    {
        Task<IEnumerable<EventViewModel>> GetUpcomingEvents();
        Task<IEnumerable<EventViewModel>> GetPastEvents();
        Task<EventDetailViewModel?> GetEventDetail(Guid id);
        Task<UpdateEventDto?> GetEventForEdit(Guid id);
        Task CreateEvent(CreateEventDto createEventDto);
        Task UpdateEvent(Guid eventId, UpdateEventDto updateEventDto);
        Task DeleteEvent(Guid id);
    }
}
