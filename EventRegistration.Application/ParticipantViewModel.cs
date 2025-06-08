using System;

namespace EventRegistration.Application
{
    public class ParticipantViewModel
    {
        public Guid Id { get; set; }
        public Guid EventId { get; set; } // To know which event this participant instance belongs to in the context of EventParticipant
        public string ParticipantType { get; set; } = string.Empty; // "Individual" or "Company"

        // Individual fields
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? FullName => ParticipantType == "Individual" ? $"{FirstName} {LastName}" : LegalName;
        public string? PersonalIdCode { get; set; }

        // Company fields
        public string? LegalName { get; set; }
        public string? RegistryCode { get; set; }
        public int? NumberOfParticipants { get; set; }

        // Common fields from EventParticipant
        public Guid PaymentMethodId { get; set; }
        public string PaymentMethodName { get; set; } = string.Empty;
        public string? EventParticipantAdditionalInfo { get; set; }
        public Guid EventParticipantId { get; set; } // Representing the Id of the link, if needed, or use composite
    }
}
