using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EventRegistration.Domain
{
    public class Event
    {
        public Event? pastEvent;

        public Event()
        {
            Name = string.Empty;
            Location = string.Empty;
            AdditionalInfo = string.Empty;
            Participants = new List<EventParticipant>();
        }

        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(300)]
        public string Name { get; set; }

        public DateTime Time { get; set; }

        [Required]
        [StringLength(100)]
        public string Location { get; set; }

        [StringLength(1000)]
        public string AdditionalInfo { get; set; }

        public ICollection<EventParticipant> Participants { get; set; } =
            new List<EventParticipant>();

        public Event(string name, DateTime eventTime, string location, string additionalInfo)
        {
            // Convert local time to UTC for consistent storage
            var utcEventTime =
                eventTime.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(eventTime, DateTimeKind.Local).ToUniversalTime()
                    : eventTime.ToUniversalTime();

            if (utcEventTime <= DateTime.UtcNow)
            {
                throw new ArgumentException("Event time must be in the future.", nameof(eventTime));
            }

            Id = Guid.NewGuid();
            Name = name;
            Time = utcEventTime;
            Location = location;
            AdditionalInfo = additionalInfo;
            Participants = new HashSet<EventParticipant>();
        }

        public void UpdateDetails(
            string name,
            DateTime eventTime,
            string location,
            string? additionalInfo
        )
        {
            // Convert local time to UTC for consistent storage
            var utcEventTime =
                eventTime.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(eventTime, DateTimeKind.Local).ToUniversalTime()
                    : eventTime.ToUniversalTime();

            if (utcEventTime <= DateTime.UtcNow)
            {
                throw new ArgumentException("Event time must be in the future.", nameof(eventTime));
            }
            Name = name;
            Time = utcEventTime;
            Location = location;
            AdditionalInfo = additionalInfo ?? string.Empty;
        }
    }
}
