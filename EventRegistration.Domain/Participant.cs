using System;
using System.Collections.Generic; // Required for ICollection

namespace EventRegistration.Domain
{
    public abstract class Participant
    {
        public Guid Id { get; private set; }
        public ICollection<EventParticipant> EventParticipants { get; set; } // Added this line

        protected Participant()
        {
            Id = Guid.NewGuid();
            EventParticipants = new HashSet<EventParticipant>(); // Initialize the collection
        }
    }

    public class IndividualParticipant : Participant
    {
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string PersonalIdCode { get; private set; }

        public IndividualParticipant(string firstName, string lastName, string personalIdCode) : base()
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

        public CompanyParticipant(string legalName, string registryCode, int numberOfParticipants) : base()
        {
            LegalName = legalName;
            RegistryCode = registryCode;
            NumberOfParticipants = numberOfParticipants;
        }
    }
}
