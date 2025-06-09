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
            _mockEventRepository
                .Setup(repo => repo.GetUpcomingEvents())
                .ReturnsAsync(events.OrderBy(e => e.Time));

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
    }
}
