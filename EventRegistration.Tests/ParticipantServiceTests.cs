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
    public class ParticipantServiceTests
    {
        private readonly Mock<IParticipantRepository> _mockParticipantRepo;
        private readonly Mock<IEventRepository> _mockEventRepo;
        private readonly Mock<IEventParticipantRepository> _mockEventParticipantRepo;
        private readonly Mock<IPaymentMethodRepository> _mockPaymentMethodRepo;
        private readonly ParticipantService _service;

        public ParticipantServiceTests()
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
        public async Task AddIndividualParticipantAsync_ShouldThrowException_WhenEventNotFound()
        {
            var dto = new AddIndividualParticipantDto { EventId = Guid.NewGuid() };
            _mockEventRepo.Setup(repo => repo.GetByIdAsync(dto.EventId)).ReturnsAsync((Event?)null);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.AddIndividualParticipantAsync(dto)
            );
            Assert.Equal("Event not found. (Parameter 'EventId')", exception.Message);
        }

        [Fact]
        public async Task AddIndividualParticipantAsync_ShouldThrowException_WhenAlreadyRegistered()
        {
            var eventId = Guid.NewGuid();
            var participantId = 1;
            var dto = new AddIndividualParticipantDto
            {
                EventId = eventId,
                PersonalIdCode = "39001011234",
            };
            var existingEvent = new Event(
                "Test Event",
                DateTime.UtcNow.AddDays(1),
                "Test Location",
                ""
            );
            var existingParticipant = new IndividualParticipant("Jane", "Doe", dto.PersonalIdCode);

            typeof(Participant)
                .GetProperty(nameof(Participant.Id))
                ?.SetValue(existingParticipant, participantId);

            _mockEventRepo.Setup(repo => repo.GetByIdAsync(eventId)).ReturnsAsync(existingEvent);
            _mockPaymentMethodRepo
                .Setup(repo => repo.GetByIdAsync(dto.PaymentMethodId))
                .ReturnsAsync(new PaymentMethod("Cash"));
            _mockParticipantRepo
                .Setup(repo => repo.GetIndividualByPersonalIdCodeAsync(dto.PersonalIdCode))
                .ReturnsAsync(existingParticipant);
            _mockEventParticipantRepo
                .Setup(repo => repo.GetByEventAndParticipantAsync(eventId, existingParticipant.Id))
                .ReturnsAsync(new EventParticipant());

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.AddIndividualParticipantAsync(dto)
            );
            Assert.Equal("Participant is already registered for this event.", exception.Message);
        }

        [Fact]
        public async Task AddIndividualParticipantAsync_ShouldAddParticipant_WhenNotAlreadyRegistered()
        {
            var eventId = Guid.NewGuid();
            var dto = new AddIndividualParticipantDto
            {
                EventId = eventId,
                FirstName = "John",
                LastName = "Smith",
                PersonalIdCode = "49001011234",
                PaymentMethodId = 1,
            };
            var testEvent = new Event(
                "Test Event",
                DateTime.UtcNow.AddDays(1),
                "Test Location",
                ""
            );

            _mockEventRepo.Setup(repo => repo.GetByIdAsync(eventId)).ReturnsAsync(testEvent);
            _mockPaymentMethodRepo
                .Setup(repo => repo.GetByIdAsync(dto.PaymentMethodId))
                .ReturnsAsync(new PaymentMethod("Cash"));
            _mockParticipantRepo
                .Setup(repo => repo.GetIndividualByPersonalIdCodeAsync(dto.PersonalIdCode))
                .ReturnsAsync((IndividualParticipant?)null);
            _mockEventParticipantRepo
                .Setup(repo => repo.AddAsync(It.IsAny<EventParticipant>()))
                .Returns(Task.CompletedTask);

            await _service.AddIndividualParticipantAsync(dto);

            _mockParticipantRepo.Verify(
                repo => repo.AddAsync(It.IsAny<IndividualParticipant>()),
                Times.Once
            );
            _mockEventParticipantRepo.Verify(
                repo => repo.AddAsync(It.IsAny<EventParticipant>()),
                Times.Once
            );
        }

        [Fact]
        public async Task DeleteParticipantAsync_ShouldThrowException_WhenRegistrationNotFound()
        {
            var eventId = Guid.NewGuid();
            var participantId = 99;
            _mockEventParticipantRepo
                .Setup(repo => repo.GetByEventAndParticipantAsync(eventId, participantId))
                .ReturnsAsync((EventParticipant?)null);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.DeleteParticipantAsync(participantId, eventId)
            );
            Assert.Equal("Participant registration not found.", exception.Message);
        }

        [Fact]
        public async Task DeleteParticipantAsync_ShouldDeleteRegistration_WhenFound()
        {
            var eventId = Guid.NewGuid();
            var participantId = 1;
            var eventParticipant = new EventParticipant
            {
                EventId = eventId,
                ParticipantId = participantId,
            };

            _mockEventParticipantRepo
                .Setup(repo => repo.GetByEventAndParticipantAsync(eventId, participantId))
                .ReturnsAsync(eventParticipant);
            _mockEventParticipantRepo
                .Setup(repo => repo.DeleteAsync(eventParticipant))
                .Returns(Task.CompletedTask);

            await _service.DeleteParticipantAsync(participantId, eventId);

            _mockEventParticipantRepo.Verify(
                repo => repo.DeleteAsync(eventParticipant),
                Times.Once
            );
        }

        [Fact]
        public async Task GetPaymentMethodsAsync_ShouldReturnAllPaymentMethods()
        {
            var paymentMethods = new List<PaymentMethod>
            {
                new PaymentMethod("Cash"),
                new PaymentMethod("Bank Transfer"),
            };
            _mockPaymentMethodRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(paymentMethods);

            var result = await _service.GetPaymentMethodsAsync();
            var resultList = result.ToList();

            Assert.Equal(2, resultList.Count);
            Assert.Equal("Cash", resultList[0].Name);
        }
    }
}
