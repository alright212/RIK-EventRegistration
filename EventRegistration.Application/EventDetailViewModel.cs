using System.Collections.Generic;

namespace EventRegistration.Application
{
    /// <summary>
    /// Represents the data needed for the event details view,
    /// including the event's information and its list of participants.
    /// </summary>
    public class EventDetailViewModel
    {
        public EventViewModel Event { get; set; }
        public IEnumerable<ParticipantViewModel> Participants { get; set; }

        public EventDetailViewModel()
        {
            Event = new EventViewModel();
            Participants = new List<ParticipantViewModel>();
        }
    }
}
