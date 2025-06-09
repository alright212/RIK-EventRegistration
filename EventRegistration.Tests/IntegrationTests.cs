using System;
using System.Linq;
using System.Threading.Tasks;
using EventRegistration.Application;
using EventRegistration.Domain;
using EventRegistration.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EventRegistration.Tests
{
    public class IntegrationTests : IDisposable
    {
        private readonly EventRegistrationDbContext _context;
        private readonly EventService _eventService;
        private readonly ParticipantService _participantService;
        private readonly EventRepository _eventRepository;
        private readonly ParticipantRepository _participantRepository;
        private readonly EventParticipantRepository _eventParticipantRepository;
        private readonly PaymentMethodRepository _paymentMethodRepository;

        public IntegrationTests()
        {
            var options = new DbContextOptionsBuilder<EventRegistrationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new EventRegistrationDbContext(options);

            _eventRepository = new EventRepository(_context);
            _participantRepository = new ParticipantRepository(_context);
            _eventParticipantRepository = new EventParticipantRepository(_context);
            _paymentMethodRepository = new PaymentMethodRepository(_context);

            _eventService = new EventService(_eventRepository, _participantRepository, _context);
            _participantService = new ParticipantService(
                _participantRepository,
                _eventRepository,
                _eventParticipantRepository,
                _paymentMethodRepository
            );

            SeedTestData();
        }

        private void SeedTestData()
        {
            var paymentMethod = new PaymentMethod("Cash");
            _context.PaymentMethods.Add(paymentMethod);
            _context.SaveChanges();
        }

        [Fact]
        public async Task CreateEventAndAddParticipant_ShouldWorkEndToEnd()
        {
            // Arrange
            var createEventDto = new CreateEventDto
            {
                Name = "Integration Test Event",
                EventTime = DateTime.UtcNow.AddDays(7),
                Location = "Test Venue",
                AdditionalInfo = "Test event for integration testing",
            };

            // Act - Create Event
            await _eventService.CreateEvent(createEventDto);

            // Assert - Event was created
            var events = await _eventService.GetUpcomingEvents();
            var eventList = events.ToList();
            Assert.Single(eventList);
            Assert.Equal("Integration Test Event", eventList[0].Name);

            var eventId = eventList[0].Id;

            // Act - Add Individual Participant
            var addParticipantDto = new AddIndividualParticipantDto
            {
                EventId = eventId,
                FirstName = "John",
                LastName = "Doe",
                PersonalIdCode = "39001011234",
                PaymentMethodId = 1,
                AdditionalInfo = "Test participant",
            };

            await _participantService.AddIndividualParticipantAsync(addParticipantDto);

            // Assert - Participant was added
            var eventDetail = await _eventService.GetEventDetail(eventId);
            Assert.NotNull(eventDetail);
            Assert.Single(eventDetail.Participants);
            Assert.Equal("John", eventDetail.Participants.First().FirstName);
            Assert.Equal("Doe", eventDetail.Participants.First().LastName);
        }

        [Fact]
        public async Task AddCompanyParticipant_ShouldWorkEndToEnd()
        {
            // Arrange
            var createEventDto = new CreateEventDto
            {
                Name = "Company Event",
                EventTime = DateTime.UtcNow.AddDays(10),
                Location = "Business Center",
                AdditionalInfo = "Corporate event",
            };

            await _eventService.CreateEvent(createEventDto);
            var events = await _eventService.GetUpcomingEvents();
            var eventId = events.First().Id;

            // Act - Add Company Participant
            var addCompanyDto = new AddCompanyParticipantDto
            {
                EventId = eventId,
                LegalName = "Test Company Ltd",
                RegistryCode = "12345678",
                NumberOfParticipants = 5,
                PaymentMethodId = 1,
                AdditionalInfo = "Company registration",
            };

            await _participantService.AddCompanyParticipantAsync(addCompanyDto);

            // Assert
            var eventDetail = await _eventService.GetEventDetail(eventId);
            Assert.NotNull(eventDetail);
            Assert.Single(eventDetail.Participants);
            Assert.Equal("Company", eventDetail.Participants.First().ParticipantType);
            Assert.Equal("Test Company Ltd", eventDetail.Participants.First().LegalName);
            Assert.Equal(5, eventDetail.Participants.First().NumberOfParticipants);
        }

        [Fact]
        public async Task UpdateAndDeleteParticipant_ShouldWorkEndToEnd()
        {
            // Arrange - Create event and add participant
            var createEventDto = new CreateEventDto
            {
                Name = "Update Test Event",
                EventTime = DateTime.UtcNow.AddDays(5),
                Location = "Test Location",
            };

            await _eventService.CreateEvent(createEventDto);
            var events = await _eventService.GetUpcomingEvents();
            var eventId = events.First().Id;

            var addParticipantDto = new AddIndividualParticipantDto
            {
                EventId = eventId,
                FirstName = "Jane",
                LastName = "Smith",
                PersonalIdCode = "49001011234",
                PaymentMethodId = 1,
            };

            await _participantService.AddIndividualParticipantAsync(addParticipantDto);

            // Get participant ID
            var eventDetail = await _eventService.GetEventDetail(eventId);
            var participantId = eventDetail!.Participants.First().ParticipantId;

            // Act - Update participant
            var updateDto = new AddIndividualParticipantDto
            {
                EventId = eventId,
                FirstName = "Jane Updated",
                LastName = "Smith Updated",
                PersonalIdCode = "49001011234",
                PaymentMethodId = 1,
                AdditionalInfo = "Updated info",
            };

            await _participantService.UpdateIndividualParticipantAsync(
                participantId,
                eventId,
                updateDto
            );

            // Assert - Participant was updated
            var updatedEventDetail = await _eventService.GetEventDetail(eventId);
            Assert.Equal("Jane Updated", updatedEventDetail!.Participants.First().FirstName);
            Assert.Equal("Smith Updated", updatedEventDetail.Participants.First().LastName);

            // Act - Delete participant
            await _participantService.DeleteParticipantAsync(participantId, eventId);

            // Assert - Participant was deleted
            var finalEventDetail = await _eventService.GetEventDetail(eventId);
            Assert.Empty(finalEventDetail!.Participants);
        }

        [Fact]
        public async Task GetPastAndUpcomingEvents_ShouldSeparateCorrectly()
        {
            // Arrange - Create past and future events
            var pastEventDto = new CreateEventDto
            {
                Name = "Past Event",
                EventTime = DateTime.UtcNow.AddDays(1), // Will create as future, then manually set to past
                Location = "Past Location",
            };

            var futureEventDto = new CreateEventDto
            {
                Name = "Future Event",
                EventTime = DateTime.UtcNow.AddDays(30),
                Location = "Future Location",
            };

            await _eventService.CreateEvent(pastEventDto);
            await _eventService.CreateEvent(futureEventDto);

            // Manually set one event to past (simulating time passage)
            var allEvents = await _context.Events.ToListAsync();
            var pastEvent = allEvents.First(e => e.Name == "Past Event");
            pastEvent.Time = DateTime.UtcNow.AddDays(-1);
            await _context.SaveChangesAsync();

            // Act
            var upcomingEvents = await _eventService.GetUpcomingEvents();
            var pastEvents = await _eventService.GetPastEvents();

            // Assert
            Assert.Single(upcomingEvents);
            Assert.Single(pastEvents);
            Assert.Equal("Future Event", upcomingEvents.First().Name);
            Assert.Equal("Past Event", pastEvents.First().Name);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
