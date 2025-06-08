using System;

namespace EventRegistration.Application
{
    public class EventViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime EventTime { get; set; }
        public string Location { get; set; } = string.Empty;
        public string? AdditionalInfo { get; set; }
        public int ParticipantCount { get; set; }
    }
}
