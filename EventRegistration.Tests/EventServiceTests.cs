using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventRegistration.Application;
using EventRegistration.Domain;
using EventRegistration.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace EventRegistration.Tests
{
    public class EventServiceTests
    {
        private readonly Mock<IEventRepository> _mockEventRepository;
        private readonly Mock<IParticipantRepository> _mockParticipantRepository;
        private readonly Mock<EventRegistrationDbContext> _mockDbContext;
        private readonly IEventService _eventService;

        public EventServiceTests()
        {
            _mockEventRepository = new Mock<IEventRepository>();
            _mockParticipantRepository = new Mock<IParticipantRepository>();

            var options = new DbContextOptionsBuilder<EventRegistrationDbContext>().Options;
            _mockDbContext = new Mock<EventRegistrationDbContext>(options);

            _eventService = new EventService(
                _mockEventRepository.Object,
                _mockParticipantRepository.Object,
                _mockDbContext.Object
            );
        }

        [Fact]
        public async Task GetUpcomingEvents_ShouldReturnOrderedUpcomingEvents()
        {
            var events = new List<Event>
            {
                new Event("Future Event 2", DateTime.UtcNow.AddDays(2), "Location", ""),
                new Event("Future Event 1", DateTime.UtcNow.AddDays(1), "Location", ""),
            };
            _mockEventRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(events);

            var result = await _eventService.GetUpcomingEvents();
            var resultList = result.ToList();

            Assert.Equal(2, resultList.Count);
            Assert.Equal("Future Event 1", resultList[0].Name);
            Assert.Equal("Future Event 2", resultList[1].Name);
        }

        [Fact]
        public async Task GetPastEvents_ShouldReturnOrderedPastEvents()
        {
            var events = new List<Event>
            {
                new Event { Name = "Future Event", Time = DateTime.UtcNow.AddDays(1) },
                new Event { Name = "Past Event 1", Time = DateTime.UtcNow.AddDays(-2) },
                new Event { Name = "Past Event 2", Time = DateTime.UtcNow.AddDays(-1) },
            };
            _mockEventRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(events);

            var result = await _eventService.GetPastEvents();
            var resultList = result.ToList();

            Assert.Equal(2, resultList.Count);
            Assert.Equal("Past Event 2", resultList[0].Name);
            Assert.Equal("Past Event 1", resultList[1].Name);
        }

        [Fact]
        public async Task DeleteEvent_ShouldThrowException_WhenDeletingPastEvent()
        {
            var pastEvent = new Event
            {
                Id = Guid.NewGuid(),
                Name = "Past Event",
                Time = DateTime.UtcNow.AddDays(-1),
            };
            _mockEventRepository
                .Setup(repo => repo.GetByIdAsync(pastEvent.Id))
                .ReturnsAsync(pastEvent);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _eventService.DeleteEvent(pastEvent.Id)
            );
        }

        [Fact]
        public async Task DeleteEvent_ShouldRemoveEvent_WhenEventIsInFuture()
        {
            var futureEvent = new Event("Future Event", DateTime.UtcNow.AddDays(1), "Location", "");
            _mockEventRepository
                .Setup(repo => repo.GetByIdAsync(futureEvent.Id))
                .ReturnsAsync(futureEvent);

            await _eventService.DeleteEvent(futureEvent.Id);

            _mockEventRepository.Verify(repo => repo.RemoveAsync(futureEvent), Times.Once);
        }

        [Fact]
        public async Task UpdateEvent_ShouldThrowException_WhenEventNotFound()
        {
            var nonExistentEventId = Guid.NewGuid();
            var updateDto = new UpdateEventDto();
            _mockEventRepository
                .Setup(repo => repo.GetByIdAsync(nonExistentEventId))
                .ReturnsAsync((Event?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _eventService.UpdateEvent(nonExistentEventId, updateDto)
            );
        }

        [Fact]
        public async Task GetEventDetail_ShouldReturnNull_WhenEventDoesNotExist()
        {
            var eventId = Guid.NewGuid();
            _mockEventRepository
                .Setup(repo => repo.GetEventWithParticipantsAsync(eventId))
                .ReturnsAsync((Event?)null);

            var result = await _eventService.GetEventDetail(eventId);

            Assert.Null(result);
        }

        [Fact]
        public async Task CreateEvent_ShouldCreateEvent_WhenValidData()
        {
            var createDto = new CreateEventDto
            {
                Name = "Test Event",
                EventTime = DateTime.UtcNow.AddDays(1),
                Location = "Test Location",
                AdditionalInfo = "Test additional info",
            };

            await _eventService.CreateEvent(createDto);

            _mockEventRepository.Verify(
                repo =>
                    repo.AddAsync(
                        It.Is<Event>(e =>
                            e.Name == createDto.Name
                            && e.Time == createDto.EventTime
                            && e.Location == createDto.Location
                            && e.AdditionalInfo == createDto.AdditionalInfo
                        )
                    ),
                Times.Once
            );
        }

        [Fact]
        public async Task CreateEvent_ShouldThrowException_WhenAdditionalInfoTooLong()
        {
            var createDto = new CreateEventDto
            {
                Name = "Test Event",
                EventTime = DateTime.UtcNow.AddDays(1),
                Location = "Test Location",
                AdditionalInfo = new string('x', 1001), // 1001 characters
            };

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _eventService.CreateEvent(createDto)
            );
            Assert.Equal(
                "Additional info cannot exceed 1000 characters. (Parameter 'AdditionalInfo')",
                exception.Message
            );
        }

        [Fact]
        public async Task GetEventForEdit_ShouldReturnUpdateEventDto_WhenEventExists()
        {
            var eventId = Guid.NewGuid();
            var existingEvent = new Event(
                "Test Event",
                DateTime.UtcNow.AddDays(1),
                "Test Location",
                "Test Info"
            );

            _mockEventRepository
                .Setup(repo => repo.GetByIdAsync(eventId))
                .ReturnsAsync(existingEvent);

            var result = await _eventService.GetEventForEdit(eventId);

            Assert.NotNull(result);
            Assert.Equal(existingEvent.Name, result.Name);
            Assert.Equal(existingEvent.Time.ToLocalTime(), result.EventTime);
            Assert.Equal(existingEvent.Location, result.Location);
            Assert.Equal(existingEvent.AdditionalInfo, result.AdditionalInfo);
        }

        [Fact]
        public async Task GetEventForEdit_ShouldReturnNull_WhenEventNotFound()
        {
            var eventId = Guid.NewGuid();
            _mockEventRepository
                .Setup(repo => repo.GetByIdAsync(eventId))
                .ReturnsAsync((Event?)null);

            var result = await _eventService.GetEventForEdit(eventId);

            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateEvent_ShouldUpdateEvent_WhenEventExists()
        {
            var eventId = Guid.NewGuid();
            var existingEvent = new Event(
                "Original Event",
                DateTime.UtcNow.AddDays(1),
                "Original Location",
                "Original Info"
            );
            var updateDto = new UpdateEventDto
            {
                Name = "Updated Event",
                EventTime = DateTime.UtcNow.AddDays(2),
                Location = "Updated Location",
                AdditionalInfo = "Updated Info",
            };

            _mockEventRepository
                .Setup(repo => repo.GetByIdAsync(eventId))
                .ReturnsAsync(existingEvent);

            await _eventService.UpdateEvent(eventId, updateDto);

            _mockEventRepository.Verify(repo => repo.UpdateAsync(existingEvent), Times.Once);
        }

        [Fact]
        public async Task GetEventDetail_ShouldReturnEventDetailViewModel_WhenEventExists()
        {
            var eventId = Guid.NewGuid();
            var existingEvent = new Event(
                "Test Event",
                DateTime.UtcNow.AddDays(1),
                "Test Location",
                "Test Info"
            );

            // Set the Id using reflection since it might be read-only
            typeof(Event).GetProperty(nameof(Event.Id))?.SetValue(existingEvent, eventId);

            _mockEventRepository
                .Setup(repo => repo.GetEventWithParticipantsAsync(eventId))
                .ReturnsAsync(existingEvent);

            var result = await _eventService.GetEventDetail(eventId);

            Assert.NotNull(result);
            Assert.Equal(eventId, result.Event.Id);
            Assert.Equal("Test Event", result.Event.Name);
            Assert.Equal("Test Location", result.Event.Location);
            Assert.Equal("Test Info", result.Event.AdditionalInfo);
        }

        [Fact]
        public async Task DeleteEvent_ShouldDoNothing_WhenEventNotFound()
        {
            var eventId = Guid.NewGuid();
            _mockEventRepository
                .Setup(repo => repo.GetByIdAsync(eventId))
                .ReturnsAsync((Event?)null);

            await _eventService.DeleteEvent(eventId);

            _mockEventRepository.Verify(repo => repo.RemoveAsync(It.IsAny<Event>()), Times.Never);
        }
    }
}
