using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EventRegistration.Application;
using EventRegistration.Domain;
using EventRegistration.Infrastructure;
using Xunit;

namespace EventRegistration.Tests
{
    public class DuplicateValidationIntegrationTest : IDisposable
    {
        private readonly EventRegistrationDbContext _context;
        private readonly ParticipantService _service;

        public DuplicateValidationIntegrationTest()
        {
            var options = new DbContextOptionsBuilder<EventRegistrationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new EventRegistrationDbContext(options);
            _context.Database.EnsureCreated();

            var participantRepo = new EventRegistration.Infrastructure.ParticipantRepository(_context);
            var eventRepo = new EventRegistration.Infrastructure.EventRepository(_context);
            var eventParticipantRepo = new EventRegistration.Infrastructure.EventParticipantRepository(_context);
            var paymentMethodRepo = new EventRegistration.Infrastructure.PaymentMethodRepository(_context);

            _service = new ParticipantService(participantRepo, eventRepo, eventParticipantRepo, paymentMethodRepo);
        }

        [Fact]
        public async Task AddIndividualParticipant_ShouldPreventDuplicate_WhenSamePersonalIdCodeAndEvent()
        {
            // Arrange
            var testEvent = new Event("Test Event", DateTime.UtcNow.AddDays(1), "Test Location", "");
            _context.Events.Add(testEvent);
            await _context.SaveChangesAsync();

            var dto = new AddIndividualParticipantDto
            {
                EventId = testEvent.Id,
                FirstName = "John",
                LastName = "Doe",
                PersonalIdCode = "39001011234",
                PaymentMethodId = 1
            };

            // Act - Add participant first time (should succeed)
            await _service.AddIndividualParticipantAsync(dto);

            // Act - Try to add same participant again (should throw exception)
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.AddIndividualParticipantAsync(dto)
            );

            // Assert
            Assert.Equal("Participant is already registered for this event.", exception.Message);
        }

        [Fact]
        public async Task AddCompanyParticipant_ShouldPreventDuplicate_WhenSameRegistryCodeAndEvent()
        {
            // Arrange
            var testEvent = new Event("Test Event", DateTime.UtcNow.AddDays(1), "Test Location", "");
            _context.Events.Add(testEvent);
            await _context.SaveChangesAsync();

            var dto = new AddCompanyParticipantDto
            {
                EventId = testEvent.Id,
                LegalName = "Test Company",
                RegistryCode = "12345678",
                NumberOfParticipants = 5,
                PaymentMethodId = 1
            };

            // Act - Add company first time (should succeed)
            await _service.AddCompanyParticipantAsync(dto);

            // Act - Try to add same company again (should throw exception)
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.AddCompanyParticipantAsync(dto)
            );

            // Assert
            Assert.Equal("Participant is already registered for this event.", exception.Message);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
