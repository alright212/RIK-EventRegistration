using System;

namespace EventRegistration.Domain
{
    public abstract class Participant
    {
        public Guid Id { get; private set; }
        public Guid EventId { get; private set; }

        protected Participant(Guid eventId)
        {
            Id = Guid.NewGuid();
            EventId = eventId;
        }
    }

    public class IndividualParticipant : Participant
    {
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string PersonalIdCode { get; private set; }

        public IndividualParticipant(Guid eventId, string firstName, string lastName, string personalIdCode) : base(eventId)
        {
            FirstName = firstName;
            LastName = lastName;
            PersonalIdCode = personalIdCode;
        }
    }

    public class CompanyParticipant : Participant
    {
        public string LegalName { get; private set; }
        public string RegistryCode { get; private set; }
        public int NumberOfParticipants { get; private set; }

        public CompanyParticipant(Guid eventId, string legalName, string registryCode, int numberOfParticipants) : base(eventId)
        {
            LegalName = legalName;
            RegistryCode = registryCode;
            NumberOfParticipants = numberOfParticipants;
        }
    }
}
