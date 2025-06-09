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
            var largeEventList = GenerateLargeEventListWithPastEvents(1000);
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
                // Generate events from 1 day to 2 years in the future only
                var daysOffset = random.Next(1, 730); // Only future events
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

        private static List<Event> GenerateLargeEventListWithPastEvents(int count)
        {
            var events = new List<Event>();
            var random = new Random();

            for (int i = 0; i < count; i++)
            {
                // Create event using parameterless constructor to bypass validation
                var pastEvent = new Event();

                // Set properties directly for past events
                pastEvent.Id = Guid.NewGuid();
                pastEvent.Name = $"Past Event {i}";
                pastEvent.Location = $"Location {i}";
                pastEvent.AdditionalInfo = $"Additional info for past event {i}";

                // Set a past date (1-365 days ago)
                var daysOffset = random.Next(1, 365);
                pastEvent.Time = DateTime.UtcNow.AddDays(-daysOffset);

                pastEvent.Participants = new HashSet<EventParticipant>();

                events.Add(pastEvent);
            }

            return events;
        }
    }
}
