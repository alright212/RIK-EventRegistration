using EventRegistration.Application;
using EventRegistration.Domain;
using EventRegistration.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            
            // Mock DbContext - it's a dependency in the service constructor
            var options = new DbContextOptionsBuilder<EventRegistrationDbContext>().Options;
            _mockDbContext = new Mock<EventRegistrationDbContext>(options);

            // Initialize EventService with all mocked dependencies
            _eventService = new EventService(
                _mockEventRepository.Object,
                _mockParticipantRepository.Object,
                _mockDbContext.Object
            );
        }

        [Fact]
        public async Task GetUpcomingEvents_ShouldReturnOrderedUpcomingEvents()
        {
            // Arrange
            var events = new List<Event>
            {
                new Event { Id = Guid.NewGuid(), Name = "Future Event 2", Time = DateTime.UtcNow.AddDays(2) },
                new Event { Id = Guid.NewGuid(), Name = "Future Event 1", Time = DateTime.UtcNow.AddDays(1) }
            };
            _mockEventRepository.Setup(repo => repo.GetUpcomingEvents()).ReturnsAsync(events.OrderBy(e => e.Time));

            // Act
            var result = await _eventService.GetUpcomingEvents();
            var resultList = result.ToList();

            // Assert
            Assert.Equal(2, resultList.Count);
            Assert.Equal("Future Event 1", resultList[0].Name); // Check that the results are ordered by time
            Assert.Equal("Future Event 2", resultList[1].Name);
        }

        [Fact]
        public async Task GetPastEvents_ShouldReturnOrderedPastEvents()
        {
            // Arrange
            var events = new List<Event>
            {
                new Event { Id = Guid.NewGuid(), Name = "Future Event", Time = DateTime.UtcNow.AddDays(1) },
                new Event { Id = Guid.NewGuid(), Name = "Past Event 1", Time = DateTime.UtcNow.AddDays(-2) },
                new Event { Id = Guid.NewGuid(), Name = "Past Event 2", Time = DateTime.UtcNow.AddDays(-1) }
            };
            _mockEventRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(events);
        
            // Act
            var result = await _eventService.GetPastEvents();
            var resultList = result.ToList();

            // Assert
            Assert.Equal(2, resultList.Count);
            Assert.Equal("Past Event 2", resultList[0].Name); // Check descending order
            Assert.Equal("Past Event 1", resultList[1].Name);
        }

        [Fact]
        public async Task DeleteEvent_ShouldThrowException_WhenDeletingPastEvent()
        {
            // Arrange
            var pastEvent = new Event { Id = Guid.NewGuid(), Name = "Past Event", Time = DateTime.UtcNow.AddDays(-1) };
            _mockEventRepository.Setup(repo => repo.GetByIdAsync(pastEvent.Id)).ReturnsAsync(pastEvent);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _eventService.DeleteEvent(pastEvent.Id));
        }

        [Fact]
        public async Task DeleteEvent_ShouldRemoveEvent_WhenEventIsInFuture()
        {
            // Arrange
            var futureEventId = Guid.NewGuid();
            var futureEvent = new Event { Id = futureEventId, Name = "Future Event", Time = DateTime.UtcNow.AddDays(1) };
            _mockEventRepository.Setup(repo => repo.GetByIdAsync(futureEventId)).ReturnsAsync(futureEvent);

            // Act
            await _eventService.DeleteEvent(futureEventId);

            // Assert
            // Verify that the RemoveAsync method was called exactly once with the correct event object
            _mockEventRepository.Verify(repo => repo.RemoveAsync(futureEvent), Times.Once);
        }
        
        [Fact]
        public async Task UpdateEvent_ShouldThrowException_WhenEventNotFound()
        {
            // Arrange
            var nonExistentEventId = Guid.NewGuid();
            var updateDto = new UpdateEventDto();
            _mockEventRepository.Setup(repo => repo.GetByIdAsync(nonExistentEventId)).ReturnsAsync((Event)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _eventService.UpdateEvent(nonExistentEventId, updateDto));
        }
        
        [Fact]
        public async Task GetEventDetail_ShouldReturnNull_WhenEventDoesNotExist()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            _mockEventRepository.Setup(repo => repo.GetEventWithParticipantsAsync(eventId)).ReturnsAsync((Event)null);

            // Act
            var result = await _eventService.GetEventDetail(eventId);

            // Assert
            Assert.Null(result);
        }
    }
}