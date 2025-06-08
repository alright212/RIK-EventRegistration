using System;
using System.ComponentModel.DataAnnotations;

namespace EventRegistration.Application
{
    public class AddIndividualParticipantDto
    {
        [Required]
        public Guid EventId { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^[3-6]\d{10}$", ErrorMessage = "Invalid Estonian Personal ID Code.")]
        public string PersonalIdCode { get; set; } = string.Empty;

        [Required]
        // FIX: Changed PaymentMethodId from Guid to int
        public int PaymentMethodId { get; set; }

        [StringLength(1500)]
        public string? AdditionalInfo { get; set; }
    }
}