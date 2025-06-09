using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public class PerformanceTests
    {
        private readonly Mock<IEventRepository> _mockEventRepository;
        private readonly Mock<IParticipantRepository> _mockParticipantRepository;
        private readonly Mock<EventRegistrationDbContext> _mockDbContext;
        private readonly IEventService _eventService;

        public PerformanceTests()
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
        public async Task GetUpcomingEvents_ShouldHandleLargeDataSet()
        {
            // Arrange
            var largeEventList = GenerateLargeEventList(1000);
            _mockEventRepository
                .Setup(repo => repo.GetUpcomingEvents())
                .ReturnsAsync(largeEventList.OrderBy(e => e.Time));

            // Act
            var stopwatch = Stopwatch.StartNew();
            var result = await _eventService.GetUpcomingEvents();
            stopwatch.Stop();

            // Assert
            Assert.True(stopwatch.ElapsedMilliseconds < 1000); // Should complete within 1 second
            Assert.Equal(1000, result.Count());
        }

        [Fact]
        public async Task GetPastEvents_ShouldHandleLargeDataSet()
        {
            // Arrange
            var largeEventList = GenerateLargeEventList(1000);
            _mockEventRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(largeEventList);

            // Act
            var stopwatch = Stopwatch.StartNew();
            var result = await _eventService.GetPastEvents();
            stopwatch.Stop();

            // Assert
            Assert.True(stopwatch.ElapsedMilliseconds < 1000); // Should complete within 1 second
            var pastEvents = result.ToList();
            Assert.True(pastEvents.Count > 0);
        }

        private static List<Event> GenerateLargeEventList(int count)
        {
            var events = new List<Event>();
            var random = new Random();

            for (int i = 0; i < count; i++)
            {
                var daysOffset = random.Next(-365, 365); // Events from past year to next year
                var eventTime = DateTime.UtcNow.AddDays(daysOffset);

                events.Add(
                    new Event(
                        $"Event {i}",
                        eventTime,
                        $"Location {i}",
                        $"Additional info for event {i}"
                    )
                );
            }

            return events;
        }
    }
}
