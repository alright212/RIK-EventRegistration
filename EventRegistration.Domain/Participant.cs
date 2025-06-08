using System;
using System.Collections.Generic; 

namespace EventRegistration.Domain
{
    public abstract class Participant
    {
        public Guid Id { get; private set; }
        public ICollection<EventParticipant> EventParticipants { get; set; } 

        protected Participant()
        {
            Id = Guid.NewGuid();
            EventParticipants = new HashSet<EventParticipant>(); 
        }
    }

    public class IndividualParticipant : Participant
    {
        public string FirstName { get; set; } 
        public string LastName { get; set; } 
        public string PersonalIdCode { get; set; } 

        public IndividualParticipant(string firstName, string lastName, string personalIdCode) : base()
        {
            FirstName = firstName;
            LastName = lastName;
            PersonalIdCode = personalIdCode;
        }
    }

    public class CompanyParticipant : Participant
    {
        public string LegalName { get; set; } 
        public string RegistryCode { get; set; } 
        public int NumberOfParticipants { get; set; } 

        public CompanyParticipant(string legalName, string registryCode, int numberOfParticipants) : base()
        {
            LegalName = legalName;
            RegistryCode = registryCode;
            NumberOfParticipants = numberOfParticipants;
        }
    }
}
