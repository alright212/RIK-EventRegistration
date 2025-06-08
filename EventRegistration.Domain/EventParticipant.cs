using System;

namespace EventRegistration.Domain
{
    public class EventParticipant
    {
        public Guid EventId { get; set; }
        public Event Event { get; set; }

        public Guid ParticipantId { get; set; }
        public Participant Participant { get; set; }

        public Guid PaymentMethodId { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string AdditionalInfo { get; set; } // Participant's additional info for THIS event
    }
}
