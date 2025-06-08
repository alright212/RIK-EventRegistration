using System;

namespace EventRegistration.Domain
{
    public class EventParticipant
    {
        // Foreign key for Event (which uses Guid)
        public Guid EventId { get; set; }
        public Event? Event { get; set; } 

        // Foreign key for Participant (which uses int)
        public int ParticipantId { get; set; }
        public Participant? Participant { get; set; } 

        // Foreign key for PaymentMethod (which uses int)
        public int PaymentMethodId { get; set; }
        public PaymentMethod? PaymentMethod { get; set; } 
        
        public string? AdditionalInfo { get; set; } 
    }
}