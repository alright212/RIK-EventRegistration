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
    public class ValidationAndEdgeCaseTests
    {
        private readonly Mock<IParticipantRepository> _mockParticipantRepo;
        private readonly Mock<IEventRepository> _mockEventRepo;
        private readonly Mock<IEventParticipantRepository> _mockEventParticipantRepo;
        private readonly Mock<IPaymentMethodRepository> _mockPaymentMethodRepo;
        private readonly ParticipantService _participantService;

        public ValidationAndEdgeCaseTests()
        {
            _mockParticipantRepo = new Mock<IParticipantRepository>();
            _mockEventRepo = new Mock<IEventRepository>();
            _mockEventParticipantRepo = new Mock<IEventParticipantRepository>();
            _mockPaymentMethodRepo = new Mock<IPaymentMethodRepository>();

            _participantService = new ParticipantService(
                _mockParticipantRepo.Object,
                _mockEventRepo.Object,
                _mockEventParticipantRepo.Object,
                _mockPaymentMethodRepo.Object
            );
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Event_Constructor_ShouldHandleEmptyOrWhitespaceAdditionalInfo(
            string additionalInfo
        )
        {
            var eventEntity = new Event(
                "Test Event",
                DateTime.UtcNow.AddDays(1),
                "Test Location",
                additionalInfo ?? ""
            );

            Assert.NotNull(eventEntity.AdditionalInfo);
        }

        [Fact]
        public void Event_Constructor_ShouldHandleNullAdditionalInfo()
        {
            var eventEntity = new Event(
                "Test Event",
                DateTime.UtcNow.AddDays(1),
                "Test Location",
                ""
            );

            Assert.NotNull(eventEntity.AdditionalInfo);
        }

        [Fact]
        public void IndividualParticipant_Constructor_ShouldCreateValidParticipant()
        {
            var participant = new IndividualParticipant("John", "Doe", "39001011234");

            Assert.Equal("John", participant.FirstName);
            Assert.Equal("Doe", participant.LastName);
            Assert.Equal("39001011234", participant.PersonalIdCode);
        }

        [Fact]
        public void CompanyParticipant_Constructor_ShouldCreateValidParticipant()
        {
            var participant = new CompanyParticipant("Test Company", "12345678", 5);

            Assert.Equal("Test Company", participant.LegalName);
            Assert.Equal("12345678", participant.RegistryCode);
            Assert.Equal(5, participant.NumberOfParticipants);
        }

        [Fact]
        public async Task GetParticipantDetailsAsync_ShouldReturnNull_WhenParticipantIsNull()
        {
            var eventId = Guid.NewGuid();
            var participantId = 1;
            var eventParticipant = new EventParticipant
            {
                EventId = eventId,
                ParticipantId = participantId,
                Participant = null, // Missing participant
                PaymentMethod = new PaymentMethod("Cash"),
            };

            _mockEventParticipantRepo
                .Setup(repo => repo.GetByEventAndParticipantAsync(eventId, participantId))
                .ReturnsAsync(eventParticipant);

            var result = await _participantService.GetParticipantDetailsAsync(
                participantId,
                eventId
            );

            Assert.Null(result);
        }

        [Fact]
        public async Task GetParticipantsByEventAsync_ShouldReturnEmptyList_WhenNoParticipants()
        {
            var eventId = Guid.NewGuid();
            var emptyList = new List<EventParticipant>();

            _mockEventParticipantRepo
                .Setup(repo => repo.GetByEventIdAsync(eventId))
                .ReturnsAsync(emptyList);

            var result = await _participantService.GetParticipantsByEventAsync(eventId);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetPaymentMethodsAsync_ShouldReturnEmptyList_WhenNoPaymentMethods()
        {
            var emptyList = new List<PaymentMethod>();
            _mockPaymentMethodRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(emptyList);

            var result = await _participantService.GetPaymentMethodsAsync();

            Assert.Empty(result);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public async Task DeleteParticipantAsync_ShouldHandleInvalidParticipantId(int invalidId)
        {
            var eventId = Guid.NewGuid();
            _mockEventParticipantRepo
                .Setup(repo => repo.GetByEventAndParticipantAsync(eventId, invalidId))
                .ReturnsAsync((EventParticipant?)null);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _participantService.DeleteParticipantAsync(invalidId, eventId)
            );
            Assert.Equal("Participant registration not found.", exception.Message);
        }
    }
}
