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

        [Fact]
        public async Task AddCompanyParticipantAsync_ShouldThrowException_WhenEventNotFound()
        {
            var dto = new AddCompanyParticipantDto { EventId = Guid.NewGuid() };
            _mockEventRepo.Setup(repo => repo.GetByIdAsync(dto.EventId)).ReturnsAsync((Event?)null);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.AddCompanyParticipantAsync(dto)
            );
            Assert.Equal("Event not found. (Parameter 'EventId')", exception.Message);
        }

        [Fact]
        public async Task AddCompanyParticipantAsync_ShouldThrowException_WhenPaymentMethodNotFound()
        {
            var dto = new AddCompanyParticipantDto
            {
                EventId = Guid.NewGuid(),
                PaymentMethodId = 999,
            };
            var testEvent = new Event(
                "Test Event",
                DateTime.UtcNow.AddDays(1),
                "Test Location",
                ""
            );

            _mockEventRepo.Setup(repo => repo.GetByIdAsync(dto.EventId)).ReturnsAsync(testEvent);
            _mockPaymentMethodRepo
                .Setup(repo => repo.GetByIdAsync(dto.PaymentMethodId))
                .ReturnsAsync((PaymentMethod?)null);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.AddCompanyParticipantAsync(dto)
            );
            Assert.Equal(
                "Payment method not found. (Parameter 'PaymentMethodId')",
                exception.Message
            );
        }

        [Fact]
        public async Task AddCompanyParticipantAsync_ShouldAddNewCompanyParticipant_WhenNotExisting()
        {
            var eventId = Guid.NewGuid();
            var dto = new AddCompanyParticipantDto
            {
                EventId = eventId,
                LegalName = "Test Company",
                RegistryCode = "12345678",
                NumberOfParticipants = 5,
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
                .Setup(repo => repo.GetCompanyByRegistryCodeAsync(dto.RegistryCode))
                .ReturnsAsync((CompanyParticipant?)null);
            _mockEventParticipantRepo
                .Setup(repo => repo.AddAsync(It.IsAny<EventParticipant>()))
                .Returns(Task.CompletedTask);

            await _service.AddCompanyParticipantAsync(dto);

            _mockParticipantRepo.Verify(
                repo => repo.AddAsync(It.IsAny<CompanyParticipant>()),
                Times.Once
            );
            _mockEventParticipantRepo.Verify(
                repo => repo.AddAsync(It.IsAny<EventParticipant>()),
                Times.Once
            );
        }

        [Fact]
        public async Task AddCompanyParticipantAsync_ShouldThrowException_WhenAlreadyRegistered()
        {
            var eventId = Guid.NewGuid();
            var participantId = 1;
            var dto = new AddCompanyParticipantDto
            {
                EventId = eventId,
                LegalName = "Test Company",
                RegistryCode = "12345678",
                NumberOfParticipants = 5,
                PaymentMethodId = 1,
            };
            var existingEvent = new Event(
                "Test Event",
                DateTime.UtcNow.AddDays(1),
                "Test Location",
                ""
            );
            var existingParticipant = new CompanyParticipant(
                dto.LegalName,
                dto.RegistryCode,
                dto.NumberOfParticipants
            );

            typeof(Participant)
                .GetProperty(nameof(Participant.Id))
                ?.SetValue(existingParticipant, participantId);

            _mockEventRepo.Setup(repo => repo.GetByIdAsync(eventId)).ReturnsAsync(existingEvent);
            _mockPaymentMethodRepo
                .Setup(repo => repo.GetByIdAsync(dto.PaymentMethodId))
                .ReturnsAsync(new PaymentMethod("Cash"));
            _mockParticipantRepo
                .Setup(repo => repo.GetCompanyByRegistryCodeAsync(dto.RegistryCode))
                .ReturnsAsync(existingParticipant);
            _mockEventParticipantRepo
                .Setup(repo => repo.GetByEventAndParticipantAsync(eventId, existingParticipant.Id))
                .ReturnsAsync(new EventParticipant());

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.AddCompanyParticipantAsync(dto)
            );
            Assert.Equal("Participant is already registered for this event.", exception.Message);
        }

        [Fact]
        public async Task AddIndividualParticipantAsync_ShouldThrowException_WhenPaymentMethodNotFound()
        {
            var dto = new AddIndividualParticipantDto
            {
                EventId = Guid.NewGuid(),
                PaymentMethodId = 999,
            };
            var testEvent = new Event(
                "Test Event",
                DateTime.UtcNow.AddDays(1),
                "Test Location",
                ""
            );

            _mockEventRepo.Setup(repo => repo.GetByIdAsync(dto.EventId)).ReturnsAsync(testEvent);
            _mockPaymentMethodRepo
                .Setup(repo => repo.GetByIdAsync(dto.PaymentMethodId))
                .ReturnsAsync((PaymentMethod?)null);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.AddIndividualParticipantAsync(dto)
            );
            Assert.Equal(
                "Payment method not found. (Parameter 'PaymentMethodId')",
                exception.Message
            );
        }

        [Fact]
        public async Task GetParticipantDetailsAsync_ShouldReturnParticipantViewModel_WhenFound()
        {
            var eventId = Guid.NewGuid();
            var participantId = 1;
            var participant = new IndividualParticipant("John", "Doe", "39001011234");

            // Set the participant ID using reflection
            typeof(Participant)
                .GetProperty(nameof(Participant.Id))
                ?.SetValue(participant, participantId);

            var paymentMethod = new PaymentMethod("Cash");
            var eventParticipant = new EventParticipant
            {
                EventId = eventId,
                ParticipantId = participantId,
                Participant = participant,
                PaymentMethod = paymentMethod,
                AdditionalInfo = "Test additional info",
            };

            _mockEventParticipantRepo
                .Setup(repo => repo.GetByEventAndParticipantAsync(eventId, participantId))
                .ReturnsAsync(eventParticipant);

            var result = await _service.GetParticipantDetailsAsync(participantId, eventId);

            Assert.NotNull(result);
            Assert.Equal(eventId, result.EventId);
            Assert.Equal(participantId, result.ParticipantId);
            Assert.Equal("Individual", result.ParticipantType);
            Assert.Equal("John", result.FirstName);
            Assert.Equal("Doe", result.LastName);
            Assert.Equal("39001011234", result.PersonalIdCode);
        }

        [Fact]
        public async Task GetParticipantDetailsAsync_ShouldReturnNull_WhenNotFound()
        {
            var eventId = Guid.NewGuid();
            var participantId = 1;

            _mockEventParticipantRepo
                .Setup(repo => repo.GetByEventAndParticipantAsync(eventId, participantId))
                .ReturnsAsync((EventParticipant?)null);

            var result = await _service.GetParticipantDetailsAsync(participantId, eventId);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetParticipantsByEventAsync_ShouldReturnParticipants()
        {
            var eventId = Guid.NewGuid();
            var participant1 = new IndividualParticipant("John", "Doe", "39001011234");
            var participant2 = new CompanyParticipant("Test Company", "12345678", 5);
            var paymentMethod = new PaymentMethod("Cash");

            var eventParticipants = new List<EventParticipant>
            {
                new EventParticipant
                {
                    EventId = eventId,
                    ParticipantId = 1,
                    Participant = participant1,
                    PaymentMethod = paymentMethod,
                },
                new EventParticipant
                {
                    EventId = eventId,
                    ParticipantId = 2,
                    Participant = participant2,
                    PaymentMethod = paymentMethod,
                },
            };

            _mockEventParticipantRepo
                .Setup(repo => repo.GetByEventIdAsync(eventId))
                .ReturnsAsync(eventParticipants);

            var result = await _service.GetParticipantsByEventAsync(eventId);
            var resultList = result.ToList();

            Assert.Equal(2, resultList.Count);
            Assert.Equal("Individual", resultList[0].ParticipantType);
            Assert.Equal("Company", resultList[1].ParticipantType);
        }

        [Fact]
        public async Task UpdateIndividualParticipantAsync_ShouldThrowException_WhenRegistrationNotFound()
        {
            var eventId = Guid.NewGuid();
            var participantId = 1;
            var dto = new AddIndividualParticipantDto();

            _mockEventParticipantRepo
                .Setup(repo => repo.GetByEventAndParticipantAsync(eventId, participantId))
                .ReturnsAsync((EventParticipant?)null);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.UpdateIndividualParticipantAsync(participantId, eventId, dto)
            );
            Assert.Equal("Participant registration not found.", exception.Message);
        }

        [Fact]
        public async Task UpdateIndividualParticipantAsync_ShouldThrowException_WhenNotIndividualParticipant()
        {
            var eventId = Guid.NewGuid();
            var participantId = 1;
            var dto = new AddIndividualParticipantDto();
            var companyParticipant = new CompanyParticipant("Test Company", "12345678", 5);
            var eventParticipant = new EventParticipant
            {
                EventId = eventId,
                ParticipantId = participantId,
                Participant = companyParticipant,
            };

            _mockEventParticipantRepo
                .Setup(repo => repo.GetByEventAndParticipantAsync(eventId, participantId))
                .ReturnsAsync(eventParticipant);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.UpdateIndividualParticipantAsync(participantId, eventId, dto)
            );
            Assert.Equal("Participant is not an individual.", exception.Message);
        }

        [Fact]
        public async Task UpdateCompanyParticipantAsync_ShouldThrowException_WhenNotCompanyParticipant()
        {
            var eventId = Guid.NewGuid();
            var participantId = 1;
            var dto = new AddCompanyParticipantDto();
            var individualParticipant = new IndividualParticipant("John", "Doe", "39001011234");
            var eventParticipant = new EventParticipant
            {
                EventId = eventId,
                ParticipantId = participantId,
                Participant = individualParticipant,
            };

            _mockEventParticipantRepo
                .Setup(repo => repo.GetByEventAndParticipantAsync(eventId, participantId))
                .ReturnsAsync(eventParticipant);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.UpdateCompanyParticipantAsync(participantId, eventId, dto)
            );
            Assert.Equal("Participant is not a company.", exception.Message);
        }

        [Fact]
        public async Task AddIndividualParticipantAsync_ShouldThrowException_WhenDtoIsNull()
        {
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.AddIndividualParticipantAsync(null!)
            );
            Assert.Equal("dto", exception.ParamName);
        }

        [Fact]
        public async Task AddCompanyParticipantAsync_ShouldThrowException_WhenDtoIsNull()
        {
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.AddCompanyParticipantAsync(null!)
            );
            Assert.Equal("dto", exception.ParamName);
        }
    }
}
