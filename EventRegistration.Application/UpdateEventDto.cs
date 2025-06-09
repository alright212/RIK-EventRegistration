using System;
using System.ComponentModel.DataAnnotations;

namespace EventRegistration.Application
{
    public class UpdateEventDto
    {
        [Required]
        [StringLength(200, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public DateTime EventTime { get; set; }

        [Required]
        [StringLength(1000, MinimumLength = 3)]
        public string Location { get; set; } = string.Empty;

        [StringLength(10000)] // Increased max length based on typical needs for additional info
        public string? AdditionalInfo { get; set; }
    }
}
