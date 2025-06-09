using System;
using System.ComponentModel.DataAnnotations;

namespace EventRegistration.Application
{
    public class UpdateEventDto
    {
        [Required(ErrorMessage = "Event name is required.")]
        [StringLength(
            100,
            MinimumLength = 3,
            ErrorMessage = "Event name must be between 3 and 100 characters."
        )]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Event time is required.")]
        public DateTime EventTime { get; set; }

        [Required(ErrorMessage = "Location is required.")]
        [StringLength(
            100,
            MinimumLength = 3,
            ErrorMessage = "Location must be between 3 and 100 characters."
        )]
        public string Location { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Additional information cannot exceed 1000 characters.")]
        public string? AdditionalInfo { get; set; }
    }
}
