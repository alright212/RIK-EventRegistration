namespace EventRegistration.Application
{
    public class CompanyParticipantLookupDto
    {
        public string LegalName { get; set; } = string.Empty;
        public string RegistryCode { get; set; } = string.Empty;
        public int NumberOfParticipants { get; set; }
    }
}
