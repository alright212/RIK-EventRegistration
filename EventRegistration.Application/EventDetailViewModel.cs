using System.Collections.Generic;

namespace EventRegistration.Application
{
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
