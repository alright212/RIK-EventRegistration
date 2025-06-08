using System;

namespace EventRegistration.Domain
{
    public class Event
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public DateTime EventTime { get; private set; }
        public string Location { get; private set; }
        public string AdditionalInfo { get; private set; }

        public Event(string name, DateTime eventTime, string location, string additionalInfo)
        {
            if (eventTime <= DateTime.UtcNow)
            {
                throw new ArgumentException("Event time must be in the future.", nameof(eventTime));
            }

            Id = Guid.NewGuid();
            Name = name;
            EventTime = eventTime;
            Location = location;
            AdditionalInfo = additionalInfo;
        }
    }
}
