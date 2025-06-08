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

            
            modelBuilder.Entity<EventParticipant>()
                .HasKey(ep => new { ep.EventId, ep.ParticipantId });

            
            // Correct the navigation property name from e.EventParticipants to e.Participants
            modelBuilder.Entity<EventParticipant>()
                .HasOne(ep => ep.Event)
                .WithMany(e => e.Participants) 
                .HasForeignKey(ep => ep.EventId);

            modelBuilder.Entity<EventParticipant>()
                .HasOne(ep => ep.Participant)
                .WithMany(p => p.EventParticipants)
                .HasForeignKey(ep => ep.ParticipantId);

            
            modelBuilder.Entity<Participant>()
                .HasDiscriminator<string>("ParticipantType")
                .HasValue<IndividualParticipant>("Individual")
                .HasValue<CompanyParticipant>("Company");
        }
    }
}