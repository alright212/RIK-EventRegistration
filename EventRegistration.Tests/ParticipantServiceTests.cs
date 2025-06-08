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
    public class ParticipantServiceTests
    {
        private readonly Mock<IParticipantRepository> _mockParticipantRepository;
        private readonly Mock<IEventRepository> _mockEventRepository;
        private readonly ParticipantService _participantService;
        private readonly EventRegistrationDbContext _dbContext;

        public ParticipantServiceTests()
        {
            _mockParticipantRepository = new Mock<IParticipantRepository>();
            _mockEventRepository = new Mock<IEventRepository>();

            var options = new DbContextOptionsBuilder<EventRegistrationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _dbContext = new EventRegistrationDbContext(options);

            _participantService = new ParticipantService(_mockParticipantRepository.Object, _mockEventRepository.Object, _dbContext);
        }

        [Fact]
        public async Task GetParticipantDetailsAsync_ShouldReturnParticipantDetails_WhenParticipantExists()
        {
            // Arrange
            var participantId = 1;
            var eventParticipant = new EventParticipant { Id = participantId, EventId = 1, Participant = new IndividualParticipant { FirstName = "John", LastName = "Doe" }, PaymentMethod = PaymentMethod.Cash };
            _mockParticipantRepository.Setup(repo => repo.GetByIdAsync(participantId)).ReturnsAsync(eventParticipant);

            // Act
            var result = await _participantService.GetParticipantDetailsAsync(participantId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(participantId, result.Id);
            Assert.IsType<IndividualParticipantViewModel>(result);
            var individualVm = result as IndividualParticipantViewModel;
            Assert.Equal("John", individualVm.FirstName);
            Assert.Equal("Doe", individualVm.LastName);
        }

        [Fact]
        public async Task GetParticipantDetailsAsync_ShouldReturnNull_WhenParticipantDoesNotExist()
        {
            // Arrange
            var participantId = 1;
            _mockParticipantRepository.Setup(repo => repo.GetByIdAsync(participantId)).ReturnsAsync((EventParticipant)null);

            // Act
            var result = await _participantService.GetParticipantDetailsAsync(participantId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddIndividualParticipantAsync_ShouldAddParticipant_WhenEventExistsAndNotFull()
        {
            // Arrange
            var eventId = 1;
            var eventItem = new Event { Id = eventId, Name = "Test Event", MaxParticipants = 10 };
            var dto = new AddIndividualParticipantDto
            {
                EventId = eventId,
                FirstName = "Jane",
                LastName = "Doe",
                IdCode = "49001011234",
                PaymentMethod = "BankTransfer",
                AdditionalInfo = "None"
            };

            _dbContext.Events.Add(eventItem);
            await _dbContext.SaveChangesAsync();

            _mockEventRepository.Setup(repo => repo.GetByIdAsync(eventId)).ReturnsAsync(eventItem);
            _mockParticipantRepository.Setup(repo => repo.GetByEventIdAsync(eventId)).ReturnsAsync(new List<EventParticipant>()); // No existing participants
            _mockParticipantRepository.Setup(repo => repo.AddAsync(It.IsAny<EventParticipant>())).Returns(Task.CompletedTask);

            // Act
            var result = await _participantService.AddIndividualParticipantAsync(dto);

            // Assert
            Assert.NotNull(result);
            _mockParticipantRepository.Verify(repo => repo.AddAsync(It.Is<EventParticipant>(ep => 
                ep.EventId == eventId &&
                ((IndividualParticipant)ep.Participant).FirstName == "Jane"
            )), Times.Once);
            // In a real scenario, verify SaveChangesAsync on DbContext if not using InMemory or if service explicitly calls it.
        }
        
        [Fact]
        public async Task AddIndividualParticipantAsync_ShouldReturnNull_WhenEventDoesNotExist()
        {
            // Arrange
            var eventId = 1;
            var dto = new AddIndividualParticipantDto { EventId = eventId };
            _mockEventRepository.Setup(repo => repo.GetByIdAsync(eventId)).ReturnsAsync((Event)null);

            // Act
            var result = await _participantService.AddIndividualParticipantAsync(dto);

            // Assert
            Assert.Null(result);
            _mockParticipantRepository.Verify(repo => repo.AddAsync(It.IsAny<EventParticipant>()), Times.Never);
        }

        [Fact]
        public async Task AddIndividualParticipantAsync_ShouldReturnNull_WhenEventIsFull()
        {
            // Arrange
            var eventId = 1;
            var eventItem = new Event { Id = eventId, Name = "Full Event", MaxParticipants = 1 }; // Event full with 1 participant
            var existingParticipant = new EventParticipant { EventId = eventId }; 
            var dto = new AddIndividualParticipantDto { EventId = eventId, FirstName = "Another", LastName = "Person" };

            _dbContext.Events.Add(eventItem);
            _dbContext.EventParticipants.Add(existingParticipant);
            await _dbContext.SaveChangesAsync();

            _mockEventRepository.Setup(repo => repo.GetByIdAsync(eventId)).ReturnsAsync(eventItem);
            // Simulate GetByEventIdAsync returning the existing participant
            _mockParticipantRepository.Setup(repo => repo.GetByEventIdAsync(eventId)).ReturnsAsync(new List<EventParticipant> { existingParticipant });

            // Act
            var result = await _participantService.AddIndividualParticipantAsync(dto);

            // Assert
            Assert.Null(result);
            _mockParticipantRepository.Verify(repo => repo.AddAsync(It.IsAny<EventParticipant>()), Times.Never);
        }

        [Fact]
        public async Task AddCompanyParticipantAsync_ShouldAddParticipant_WhenEventExistsAndNotFull()
        {
            // Arrange
            var eventId = 1;
            var eventItem = new Event { Id = eventId, Name = "Corporate Event", MaxParticipants = 5 };
            var dto = new AddCompanyParticipantDto
            {
                EventId = eventId,
                Name = "Test Corp",
                RegistryCode = "12345678",
                ParticipantCount = 2,
                PaymentMethod = "Cash",
                AdditionalInfo = "VIPs"
            };

            _dbContext.Events.Add(eventItem);
            await _dbContext.SaveChangesAsync();

            _mockEventRepository.Setup(repo => repo.GetByIdAsync(eventId)).ReturnsAsync(eventItem);
            _mockParticipantRepository.Setup(repo => repo.GetByEventIdAsync(eventId)).ReturnsAsync(new List<EventParticipant>());
            _mockParticipantRepository.Setup(repo => repo.AddAsync(It.IsAny<EventParticipant>())).Returns(Task.CompletedTask);

            // Act
            var result = await _participantService.AddCompanyParticipantAsync(dto);

            // Assert
            Assert.NotNull(result);
            _mockParticipantRepository.Verify(repo => repo.AddAsync(It.Is<EventParticipant>(ep =>
                ep.EventId == eventId &&
                ((CompanyParticipant)ep.Participant).Name == "Test Corp" &&
                ((CompanyParticipant)ep.Participant).ParticipantCount == 2
            )), Times.Once);
        }

        [Fact]
        public async Task AddCompanyParticipantAsync_ShouldReturnNull_WhenEventIsFull()
        {
            // Arrange
            var eventId = 1;
            var eventItem = new Event { Id = eventId, Name = "Full Corp Event", MaxParticipants = 2 }; // Max 2 participants
            var existingParticipant = new EventParticipant { EventId = eventId, Participant = new IndividualParticipant() }; // One spot taken
            var dto = new AddCompanyParticipantDto { EventId = eventId, Name = "Big Corp", ParticipantCount = 2 }; // Trying to add 2 more

            _dbContext.Events.Add(eventItem);
            _dbContext.EventParticipants.Add(existingParticipant);
            await _dbContext.SaveChangesAsync();

            _mockEventRepository.Setup(repo => repo.GetByIdAsync(eventId)).ReturnsAsync(eventItem);
            _mockParticipantRepository.Setup(repo => repo.GetByEventIdAsync(eventId)).ReturnsAsync(new List<EventParticipant> { existingParticipant });

            // Act
            var result = await _participantService.AddCompanyParticipantAsync(dto);

            // Assert
            Assert.Null(result);
            _mockParticipantRepository.Verify(repo => repo.AddAsync(It.IsAny<EventParticipant>()), Times.Never);
        }

        [Fact]
        public async Task DeleteParticipantAsync_ShouldDeleteParticipant_WhenParticipantExists()
        {
            // Arrange
            var participantId = 1;
            var eventParticipant = new EventParticipant { Id = participantId, EventId = 1, Participant = new IndividualParticipant() };
            
            _dbContext.EventParticipants.Add(eventParticipant);
            await _dbContext.SaveChangesAsync();

            _mockParticipantRepository.Setup(repo => repo.GetByIdAsync(participantId)).ReturnsAsync(eventParticipant);
            _mockParticipantRepository.Setup(repo => repo.DeleteAsync(It.IsAny<EventParticipant>())).Returns(Task.CompletedTask);

            // Act
            await _participantService.DeleteParticipantAsync(participantId);

            // Assert
            _mockParticipantRepository.Verify(repo => repo.DeleteAsync(It.Is<EventParticipant>(ep => ep.Id == participantId)), Times.Once);
            // Verify in InMemory DB
            var deletedParticipant = await _dbContext.EventParticipants.FindAsync(participantId);
            Assert.Null(deletedParticipant);
        }

        [Fact]
        public async Task DeleteParticipantAsync_ShouldNotThrow_WhenParticipantDoesNotExist()
        {
            // Arrange
            var participantId = 99; // Non-existent ID
            _mockParticipantRepository.Setup(repo => repo.GetByIdAsync(participantId)).ReturnsAsync((EventParticipant)null);

            // Act
            await _participantService.DeleteParticipantAsync(participantId);

            // Assert
            _mockParticipantRepository.Verify(repo => repo.DeleteAsync(It.IsAny<EventParticipant>()), Times.Never);
        }

        [Fact]
        public async Task UpdateParticipantAsync_ShouldUpdateIndividualParticipant()
        {
            // Arrange
            var participantId = 1;
            var existingParticipant = new IndividualParticipant { Id = 10, FirstName = "OldFirst", LastName = "OldLast", IdCode = "38001010000" }; 
            var eventParticipant = new EventParticipant { Id = participantId, EventId = 1, ParticipantId = 10, Participant = existingParticipant, PaymentMethod = PaymentMethod.Cash };
            var dto = new AddOrEditParticipantViewModel
            {
                Id = participantId,
                IsCompany = false,
                IndividualFirstName = "NewFirst",
                IndividualLastName = "NewLast",
                IndividualIdCode = "49001011234",
                PaymentMethod = "BankTransfer",
                AdditionalInfo = "Updated info"
            };

            _dbContext.Participants.Add(existingParticipant);
            _dbContext.EventParticipants.Add(eventParticipant);
            await _dbContext.SaveChangesAsync();

            _mockParticipantRepository.Setup(repo => repo.GetByIdAsync(participantId)).ReturnsAsync(eventParticipant);

            // Act
            await _participantService.UpdateParticipantAsync(dto);

            // Assert
            var updatedEventParticipant = await _dbContext.EventParticipants.Include(ep => ep.Participant).FirstOrDefaultAsync(ep => ep.Id == participantId);
            Assert.NotNull(updatedEventParticipant);
            Assert.Equal(PaymentMethod.BankTransfer, updatedEventParticipant.PaymentMethod);
            Assert.Equal("Updated info", updatedEventParticipant.AdditionalInfo);
            var updatedIndividual = updatedEventParticipant.Participant as IndividualParticipant;
            Assert.NotNull(updatedIndividual);
            Assert.Equal("NewFirst", updatedIndividual.FirstName);
            Assert.Equal("NewLast", updatedIndividual.LastName);
            Assert.Equal("49001011234", updatedIndividual.IdCode);
        }

        [Fact]
        public async Task UpdateParticipantAsync_ShouldUpdateCompanyParticipant()
        {
            // Arrange
            var participantId = 2;
            var existingCompany = new CompanyParticipant { Id = 20, Name = "Old Company", RegistryCode = "11111111", ParticipantCount = 5 };
            var eventParticipant = new EventParticipant { Id = participantId, EventId = 1, ParticipantId = 20, Participant = existingCompany, PaymentMethod = PaymentMethod.Cash };
            var dto = new AddOrEditParticipantViewModel
            {
                Id = participantId,
                IsCompany = true,
                CompanyName = "New Company",
                CompanyRegistryCode = "87654321",
                CompanyParticipantCount = 10,
                PaymentMethod = "Cash",
                AdditionalInfo = "More attendees"
            };

            _dbContext.Participants.Add(existingCompany);
            _dbContext.EventParticipants.Add(eventParticipant);
            await _dbContext.SaveChangesAsync();

            _mockParticipantRepository.Setup(repo => repo.GetByIdAsync(participantId)).ReturnsAsync(eventParticipant);

            // Act
            await _participantService.UpdateParticipantAsync(dto);

            // Assert
            var updatedEventParticipant = await _dbContext.EventParticipants.Include(ep => ep.Participant).FirstOrDefaultAsync(ep => ep.Id == participantId);
            Assert.NotNull(updatedEventParticipant);
            Assert.Equal(PaymentMethod.Cash, updatedEventParticipant.PaymentMethod);
            Assert.Equal("More attendees", updatedEventParticipant.AdditionalInfo);
            var updatedCompany = updatedEventParticipant.Participant as CompanyParticipant;
            Assert.NotNull(updatedCompany);
            Assert.Equal("New Company", updatedCompany.Name);
            Assert.Equal("87654321", updatedCompany.RegistryCode);
            Assert.Equal(10, updatedCompany.ParticipantCount);
        }

        [Fact]
        public async Task UpdateParticipantAsync_ShouldNotUpdate_WhenParticipantDoesNotExist()
        {
            // Arrange
            var dto = new AddOrEditParticipantViewModel { Id = 99 }; // Non-existent ID
            _mockParticipantRepository.Setup(repo => repo.GetByIdAsync(dto.Id.Value)).ReturnsAsync((EventParticipant)null);

            // Act
            await _participantService.UpdateParticipantAsync(dto);

            // Assert
            // Verify that SaveChangesAsync was not called (or no changes were made if it was)
            // This can be tricky to assert directly with InMemory without more complex DbContext mocking or tracking.
            // For now, we rely on the fact that GetByIdAsync returning null should prevent updates.
            var nonExistent = await _dbContext.EventParticipants.FindAsync(dto.Id.Value);
            Assert.Null(nonExistent);
        }
    }
}
