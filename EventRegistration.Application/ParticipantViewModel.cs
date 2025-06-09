using System;

namespace EventRegistration.Application
{
    public class ParticipantViewModel
    {
        public Guid EventId { get; set; }
        public int ParticipantId { get; set; }
        public string ParticipantType { get; set; } = string.Empty;

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? FullName =>
            ParticipantType == "Individual" ? $"{FirstName} {LastName}" : LegalName;
        public string? PersonalIdCode { get; set; }

        public string? LegalName { get; set; }
        public string? RegistryCode { get; set; }
        public int? NumberOfParticipants { get; set; }

        // FIX: Changed PaymentMethodId from Guid to int
        public int PaymentMethodId { get; set; }
        public string PaymentMethodName { get; set; } = string.Empty;
        public string? EventParticipantAdditionalInfo { get; set; }
    }
}
