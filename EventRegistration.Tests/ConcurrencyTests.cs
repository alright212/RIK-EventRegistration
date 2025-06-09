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
    public class ConcurrencyTests
    {
        private readonly Mock<IParticipantRepository> _mockParticipantRepo;
        private readonly Mock<IEventRepository> _mockEventRepo;
        private readonly Mock<IEventParticipantRepository> _mockEventParticipantRepo;
        private readonly Mock<IPaymentMethodRepository> _mockPaymentMethodRepo;
        private readonly ParticipantService _service;

        public ConcurrencyTests()
        {
            _mockParticipantRepo = new Mock<IParticipantRepository>();
            _mockEventRepo = new Mock<IEventRepository>();
            _mockEventParticipantRepo = new Mock<IEventParticipantRepository>();
            _mockPaymentMethodRepo = new Mock<IPaymentMethodRepository>();

            _service = new ParticipantService(
                _mockParticipantRepo.Object,
                _mockEventRepo.Object,
                _mockEventParticipantRepo.Object,
                _mockPaymentMethodRepo.Object
            );
        }

        [Fact]
        public async Task AddMultipleParticipants_ShouldHandleConcurrentRequests()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var testEvent = new Event("Test Event", DateTime.UtcNow.AddDays(1), "Location", "");
            var paymentMethod = new PaymentMethod("Cash");

            _mockEventRepo.Setup(repo => repo.GetByIdAsync(eventId)).ReturnsAsync(testEvent);
            _mockPaymentMethodRepo.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(paymentMethod);
            _mockParticipantRepo
                .Setup(repo => repo.GetIndividualByPersonalIdCodeAsync(It.IsAny<string>()))
                .ReturnsAsync((IndividualParticipant?)null);
            _mockEventParticipantRepo
                .Setup(repo =>
                    repo.GetByEventAndParticipantAsync(It.IsAny<Guid>(), It.IsAny<int>())
                )
                .ReturnsAsync((EventParticipant?)null);

            // Act
            var tasks = new List<Task>();
            for (int i = 0; i < 10; i++)
            {
                var dto = new AddIndividualParticipantDto
                {
                    EventId = eventId,
                    FirstName = $"John{i}",
                    LastName = $"Doe{i}",
                    PersonalIdCode = $"3900101123{i}",
                    PaymentMethodId = 1,
                };

                tasks.Add(_service.AddIndividualParticipantAsync(dto));
            }

            // Assert
            await Task.WhenAll(tasks);

            _mockParticipantRepo.Verify(
                repo => repo.AddAsync(It.IsAny<IndividualParticipant>()),
                Times.Exactly(10)
            );
        }

        [Fact]
        public async Task GetParticipantDetails_ShouldHandleSimultaneousRequests()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var participantId = 1;
            var participant = new IndividualParticipant("John", "Doe", "39001011234");

            typeof(Participant)
                .GetProperty(nameof(Participant.Id))
                ?.SetValue(participant, participantId);

            var eventParticipant = new EventParticipant
            {
                EventId = eventId,
                ParticipantId = participantId,
                Participant = participant,
                PaymentMethod = new PaymentMethod("Cash"),
            };

            _mockEventParticipantRepo
                .Setup(repo => repo.GetByEventAndParticipantAsync(eventId, participantId))
                .ReturnsAsync(eventParticipant);

            // Act
            var tasks = new List<Task<ParticipantViewModel?>>();
            for (int i = 0; i < 20; i++)
            {
                tasks.Add(_service.GetParticipantDetailsAsync(participantId, eventId));
            }

            var results = await Task.WhenAll(tasks);

            // Assert
            Assert.All(
                results,
                result =>
                {
                    Assert.NotNull(result);
                    Assert.Equal("John", result.FirstName);
                    Assert.Equal("Doe", result.LastName);
                }
            );
        }
    }
}
