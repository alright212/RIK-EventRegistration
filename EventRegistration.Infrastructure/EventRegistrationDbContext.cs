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
        public DbSet<EventParticipant> EventParticipants { get; set; }

        public EventRegistrationDbContext(DbContextOptions<EventRegistrationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the composite primary key for the linking table
            modelBuilder.Entity<EventParticipant>()
                .HasKey(ep => new { ep.EventId, ep.ParticipantId });

            // Configure the relationships
            modelBuilder.Entity<EventParticipant>()
                .HasOne(ep => ep.Event)
                .WithMany(e => e.EventParticipants)
                .HasForeignKey(ep => ep.EventId);

            modelBuilder.Entity<EventParticipant>()
                .HasOne(ep => ep.Participant)
                .WithMany(p => p.EventParticipants)
                .HasForeignKey(ep => ep.ParticipantId);

            // Configure the TPH inheritance for Participant
            modelBuilder.Entity<Participant>()
                .HasDiscriminator<string>("ParticipantType")
                .HasValue<IndividualParticipant>("Individual")
                .HasValue<CompanyParticipant>("Company");
        }
    }
}
