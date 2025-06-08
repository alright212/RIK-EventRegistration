using System;
using System.Collections.Generic;

namespace EventRegistration.Domain
{
    public class Event
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public DateTime EventTime { get; private set; }
        public string Location { get; private set; }
        public string AdditionalInfo { get; private set; }

        public ICollection<EventParticipant> EventParticipants { get; set; }

        
        private Event()
        {
            Name = string.Empty;
            Location = string.Empty;
            AdditionalInfo = string.Empty;
            EventParticipants = new HashSet<EventParticipant>();
        }

        public Event(string name, DateTime eventTime, string location, string additionalInfo)
        {
            if (eventTime <= DateTime.UtcNow)
            {
                throw new ArgumentException("Event time must be in the future.", nameof(eventTime));
            }

            Id = Guid.NewGuid();
            Name = name;
            EventTime = eventTime;
            Location = location;
            AdditionalInfo = additionalInfo;
            EventParticipants = new HashSet<EventParticipant>();
        }

        
        
        
        public void UpdateDetails(string name, DateTime eventTime, string location, string? additionalInfo)
        {
            if (eventTime <= DateTime.UtcNow)
            {
                throw new ArgumentException("Event time must be in the future.", nameof(eventTime));
            }
            Name = name;
            EventTime = eventTime;
            Location = location;
            AdditionalInfo = additionalInfo ?? string.Empty;
        }
    }
}