using System;

namespace EventRegistration.Domain
{
    public class EventParticipant
    {
        public Guid EventId { get; set; }
        public Event? Event { get; set; } // Made nullable

        public Guid ParticipantId { get; set; }
        public Participant? Participant { get; set; } // Made nullable

        public Guid PaymentMethodId { get; set; }
        public PaymentMethod? PaymentMethod { get; set; } // Made nullable
        public string? AdditionalInfo { get; set; } // Made nullable, Participant's additional info for THIS event
    }
}
