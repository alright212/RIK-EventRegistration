using System;
using System.ComponentModel.DataAnnotations;

namespace EventRegistration.Application
{
    public class CreateEventDto
    {
        [Required(ErrorMessage = "Event name is required.")]
        [StringLength(300, ErrorMessage = "Event name cannot exceed 300 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Event time is required.")]
        public DateTime EventTime { get; set; }

        [Required(ErrorMessage = "Location is required.")]
        [StringLength(100, ErrorMessage = "Location cannot exceed 100 characters.")]
        public string Location { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Additional information cannot exceed 1000 characters.")]
        public string? AdditionalInfo { get; set; } // Max 1000 chars, made nullable to allow empty
    }
}
