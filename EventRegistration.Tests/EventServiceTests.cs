using Xunit;
using Moq;
using EventRegistration.Application;
using EventRegistration.Domain;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Microsoft.EntityFrameworkCore;
using EventRegistration.Infrastructure;

namespace EventRegistration.Tests
{
    public class EventServiceTests
    {
        private readonly Mock<IEventRepository> _mockEventRepository;
        private readonly Mock<IParticipantRepository> _mockParticipantRepository;
        private readonly EventService _eventService;
        private readonly EventRegistrationDbContext _dbContext;

        public EventServiceTests()
        {
            _mockEventRepository = new Mock<IEventRepository>();
            _mockParticipantRepository = new Mock<IParticipantRepository>();

            // Setup InMemory database
            var options = new DbContextOptionsBuilder<EventRegistrationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique name for each test run
                .Options;
            _dbContext = new EventRegistrationDbContext(options);

            _eventService = new EventService(_mockEventRepository.Object, _mockParticipantRepository.Object, _dbContext);
        }

        [Fact]
        public async Task GetAllEventsAsync_ShouldReturnAllEvents()
        {
            // Arrange
            var events = new List<Event>
            {
                new Event { Id = 1, Name = "Event 1", Time = DateTime.Now, Location = "Location 1", AdditionalInfo = "Info 1" },
                new Event { Id = 2, Name = "Event 2", Time = DateTime.Now.AddDays(1), Location = "Location 2", AdditionalInfo = "Info 2" }
            };
            _mockEventRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(events);

            // Act
            var result = await _eventService.GetAllEventsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetEventDetailsAsync_ShouldReturnEventDetails_WhenEventExists()
        {
            // Arrange
            var eventId = 1;
            var eventItem = new Event { Id = eventId, Name = "Event 1", Time = DateTime.Now, Location = "Location 1", AdditionalInfo = "Info 1" };
            _mockEventRepository.Setup(repo => repo.GetByIdAsync(eventId)).ReturnsAsync(eventItem);
            _mockParticipantRepository.Setup(repo => repo.GetByEventIdAsync(eventId)).ReturnsAsync(new List<EventParticipant>());

            // Act
            var result = await _eventService.GetEventDetailsAsync(eventId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(eventId, result.Id);
            Assert.Equal("Event 1", result.Name);
        }

        [Fact]
        public async Task GetEventDetailsAsync_ShouldReturnNull_WhenEventDoesNotExist()
        {
            // Arrange
            var eventId = 1;
            _mockEventRepository.Setup(repo => repo.GetByIdAsync(eventId)).ReturnsAsync((Event)null);

            // Act
            var result = await _eventService.GetEventDetailsAsync(eventId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateEventAsync_ShouldCreateEvent()
        {
            // Arrange
            var createEventDto = new CreateEventDto
            {
                Name = "New Event",
                Time = DateTime.Now.AddDays(5),
                Location = "New Location",
                AdditionalInfo = "New Info"
            };
            _mockEventRepository.Setup(repo => repo.AddAsync(It.IsAny<Event>())).Returns(Task.CompletedTask);

            // Act
            await _eventService.CreateEventAsync(createEventDto);

            // Assert
            _mockEventRepository.Verify(repo => repo.AddAsync(It.Is<Event>(e => 
                e.Name == createEventDto.Name &&
                e.Time == createEventDto.Time &&
                e.Location == createEventDto.Location &&
                e.AdditionalInfo == createEventDto.AdditionalInfo
            )), Times.Once);
            // In a real scenario with a real DbContext, you would also verify SaveChangesAsync was called.
            // For InMemory, the changes are immediate.
        }
        
        [Fact]
        public async Task UpdateEventAsync_ShouldUpdateEvent_WhenEventExists()
        {
            // Arrange
            var eventId = 1;
            var existingEvent = new Event { Id = eventId, Name = "Old Event", Time = DateTime.Now, Location = "Old Location" };
            var updateEventDto = new UpdateEventDto
            {
                Id = eventId,
                Name = "Updated Event",
                Time = DateTime.Now.AddDays(10),
                Location = "Updated Location",
                AdditionalInfo = "Updated Info"
            };

            _dbContext.Events.Add(existingEvent);
            await _dbContext.SaveChangesAsync(); // Save to InMemory DB

            // Act
            await _eventService.UpdateEventAsync(updateEventDto);
            
            // Assert
            var updatedEvent = await _dbContext.Events.FindAsync(eventId);
            Assert.NotNull(updatedEvent);
            Assert.Equal(updateEventDto.Name, updatedEvent.Name);
            Assert.Equal(updateEventDto.Time, updatedEvent.Time);
            Assert.Equal(updateEventDto.Location, updatedEvent.Location);
            Assert.Equal(updateEventDto.AdditionalInfo, updatedEvent.AdditionalInfo);
        }

        [Fact]
        public async Task UpdateEventAsync_ShouldNotUpdate_WhenEventDoesNotExist()
        {
            // Arrange
            var updateEventDto = new UpdateEventDto
            {
                Id = 99, // Non-existent ID
                Name = "Updated Event",
                Time = DateTime.Now.AddDays(10),
                Location = "Updated Location",
                AdditionalInfo = "Updated Info"
            };

            // Act
            await _eventService.UpdateEventAsync(updateEventDto);

            // Assert
            // Check that no event with this ID was added or modified
            var nonExistentEvent = await _dbContext.Events.FindAsync(updateEventDto.Id);
            Assert.Null(nonExistentEvent);
            // We could also verify that _mockEventRepository.UpdateAsync was not called if EventService used it directly for updates.
            // However, the current EventService implementation fetches from DbContext, modifies, and SaveChangesAsync.
        }


        [Fact]
        public async Task DeleteEventAsync_ShouldDeleteEventAndParticipants_WhenEventExists()
        {
            // Arrange
            var eventId = 1;
            var eventItem = new Event { Id = eventId, Name = "Event to Delete" };
            var participants = new List<EventParticipant>
            {
                new EventParticipant { Id = 1, EventId = eventId, Participant = new CompanyParticipant { Name = "Company 1"} },
                new EventParticipant { Id = 2, EventId = eventId, Participant = new IndividualParticipant { FirstName = "Indi", LastName = "Vidual"} }
            };

            await _dbContext.Events.AddAsync(eventItem);
            await _dbContext.EventParticipants.AddRangeAsync(participants);
            await _dbContext.SaveChangesAsync();
            
            _mockEventRepository.Setup(repo => repo.GetByIdAsync(eventId)).ReturnsAsync(eventItem);
            _mockParticipantRepository.Setup(repo => repo.GetByEventIdAsync(eventId)).ReturnsAsync(participants);
            _mockEventRepository.Setup(repo => repo.DeleteAsync(It.IsAny<Event>())).Returns(Task.CompletedTask);
            _mockParticipantRepository.Setup(repo => repo.DeleteAsync(It.IsAny<EventParticipant>())).Returns(Task.CompletedTask);


            // Act
            await _eventService.DeleteEventAsync(eventId);

            // Assert
             _mockEventRepository.Verify(repo => repo.DeleteAsync(It.Is<Event>(e => e.Id == eventId)), Times.Once);
            _mockParticipantRepository.Verify(repo => repo.DeleteAsync(It.IsAny<EventParticipant>()), Times.Exactly(participants.Count));
            // In a real scenario with a real DbContext, you would also verify SaveChangesAsync was called.
        }

        [Fact]
        public async Task DeleteEventAsync_ShouldNotThrow_WhenEventDoesNotExist()
        {
            // Arrange
            var eventId = 99; // Non-existent ID
            _mockEventRepository.Setup(repo => repo.GetByIdAsync(eventId)).ReturnsAsync((Event)null);

            // Act
            await _eventService.DeleteEventAsync(eventId);

            // Assert
            _mockEventRepository.Verify(repo => repo.DeleteAsync(It.IsAny<Event>()), Times.Never);
            _mockParticipantRepository.Verify(repo => repo.DeleteAsync(It.IsAny<EventParticipant>()), Times.Never);
        }
    }
}
