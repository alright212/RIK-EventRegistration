using System;

namespace EventRegistration.Application
{
    public class CreateEventDto
    {
        public string Name { get; set; } = string.Empty;
        public DateTime EventTime { get; set; }
        public string Location { get; set; } = string.Empty;
        public string? AdditionalInfo { get; set; } // Max 1000 chars, made nullable to allow empty
    }
}
