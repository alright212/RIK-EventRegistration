using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EventRegistration.Domain
{
    public class Event
    {
        // Add a public constructor to allow instantiation.
        public Event() 
        {
            Name = string.Empty;
            Location = string.Empty;
            AdditionalInfo = string.Empty;
            Participants = new List<EventParticipant>();
        }

        [Key]
        public Guid Id { get; set; } // Make the setter public

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public DateTime Time { get; set; } // Renamed from EventTime

        [Required]
        [StringLength(100)]
        public string Location { get; set; }

        [StringLength(1000)]
        public string AdditionalInfo { get; set; }
        
        public ICollection<EventParticipant> Participants { get; set; } = new List<EventParticipant>(); // Renamed from EventParticipants and initialized

        public Event(string name, DateTime eventTime, string location, string additionalInfo)
        {
            if (eventTime <= DateTime.UtcNow)
            {
                throw new ArgumentException("Event time must be in the future.", nameof(eventTime));
            }

            Id = Guid.NewGuid();
            Name = name;
            Time = eventTime; // Adjusted to new property name
            Location = location;
            AdditionalInfo = additionalInfo;
            Participants = new HashSet<EventParticipant>(); // Adjusted to new property name
        }

        public void UpdateDetails(string name, DateTime eventTime, string location, string? additionalInfo)
        {
            if (eventTime <= DateTime.UtcNow)
            {
                throw new ArgumentException("Event time must be in the future.", nameof(eventTime));
            }
            Name = name;
            Time = eventTime; // Adjusted to new property name
            Location = location;
            AdditionalInfo = additionalInfo ?? string.Empty;
        }
    }
}