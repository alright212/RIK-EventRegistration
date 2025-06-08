using EventRegistration.Domain;
using Microsoft.EntityFrameworkCore;

namespace EventRegistration.Infrastructure
{
    public class EventRegistrationDbContext : DbContext
    {
        public DbSet<Event> Events { get; set; }
        public DbSet<Participant> Participants { get; set; }
        public DbSet<IndividualParticipant> IndividualParticipants { get; set; }
        public DbSet<CompanyParticipant> CompanyParticipants { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }

        public EventRegistrationDbContext(DbContextOptions<EventRegistrationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the TPH inheritance for Participant
            modelBuilder.Entity<Participant>()
                .HasDiscriminator<string>("ParticipantType")
                .HasValue<IndividualParticipant>("Individual")
                .HasValue<CompanyParticipant>("Company");
        }
    }
}
