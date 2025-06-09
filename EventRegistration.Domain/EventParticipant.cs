using System;

namespace EventRegistration.Domain
{
    public class EventParticipant
    {
        public Guid EventId { get; set; }
        public Event? Event { get; set; }

        public int ParticipantId { get; set; }
        public Participant? Participant { get; set; }

        public int PaymentMethodId { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }

        public string? AdditionalInfo { get; set; }

        // Event-specific number of participants for company registrations
        public int? NumberOfParticipants { get; set; }
    }
}
