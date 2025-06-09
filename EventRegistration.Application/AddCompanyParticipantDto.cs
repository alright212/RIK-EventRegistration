using System;
using System.ComponentModel.DataAnnotations;

namespace EventRegistration.Application
{
    public class AddCompanyParticipantDto
    {
        [Required]
        public Guid EventId { get; set; }

        [Required]
        [StringLength(200)]
        public string LegalName { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^\d{8}$", ErrorMessage = "Invalid Estonian Registry Code.")]
        public string RegistryCode { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Number of participants must be at least 1.")]
        public int NumberOfParticipants { get; set; }

        [Required]
        public int PaymentMethodId { get; set; }

        [StringLength(5000)]
        public string? AdditionalInfo { get; set; }
    }
}
