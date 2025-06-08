using System;

namespace EventRegistration.Application
{
    public class CreateEventDto
    {
        public string Name { get; set; }
        public DateTime EventTime { get; set; }
        public string Location { get; set; }
        public string AdditionalInfo { get; set; } // Max 1000 chars 
    }
}
