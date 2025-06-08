using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventRegistration.Application
{
    public interface IEventService
    {
        Task<IEnumerable<EventViewModel>> GetUpcomingEvents();
        Task<IEnumerable<EventViewModel>> GetPastEvents();
        Task<EventDetailViewModel?> GetEventDetail(Guid id); // Changed int to Guid, made nullable
        Task<UpdateEventDto?> GetEventForEdit(Guid id); // Changed int to Guid, made nullable
        Task CreateEvent(CreateEventDto createEventDto);
        Task UpdateEvent(Guid eventId, UpdateEventDto updateEventDto); // Changed signature
        Task DeleteEvent(Guid id); // Changed int to Guid
    }
}
