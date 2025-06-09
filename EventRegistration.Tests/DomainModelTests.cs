using System;
using EventRegistration.Domain;
using Xunit;

namespace EventRegistration.Tests
{
    public class DomainModelTests
    {
        [Fact]
        public void Event_Constructor_ShouldCreateEventWithCorrectProperties()
        {
            var name = "Test Event";
            var eventTime = DateTime.UtcNow.AddDays(1);
            var location = "Test Location";
            var additionalInfo = "Test additional info";

            var eventEntity = new Event(name, eventTime, location, additionalInfo);

            Assert.Equal(name, eventEntity.Name);
            Assert.Equal(eventTime, eventEntity.Time);
            Assert.Equal(location, eventEntity.Location);
            Assert.Equal(additionalInfo, eventEntity.AdditionalInfo);
            Assert.NotEqual(Guid.Empty, eventEntity.Id);
        }

        [Fact]
        public void Event_Constructor_ShouldThrowException_WhenEventTimeInPast()
        {
            var name = "Test Event";
            var eventTime = DateTime.UtcNow.AddDays(-1);
            var location = "Test Location";
            var additionalInfo = "Test additional info";

            var exception = Assert.Throws<ArgumentException>(() =>
                new Event(name, eventTime, location, additionalInfo)
            );
            Assert.Equal(
                "Event time must be in the future. (Parameter 'eventTime')",
                exception.Message
            );
        }

        [Fact]
        public void Event_UpdateDetails_ShouldUpdatePropertiesCorrectly()
        {
            var eventEntity = new Event(
                "Original Event",
                DateTime.UtcNow.AddDays(1),
                "Original Location",
                "Original Info"
            );

            var newName = "Updated Event";
            var newTime = DateTime.UtcNow.AddDays(2);
            var newLocation = "Updated Location";
            var newAdditionalInfo = "Updated Info";

            eventEntity.UpdateDetails(newName, newTime, newLocation, newAdditionalInfo);

            Assert.Equal(newName, eventEntity.Name);
            Assert.Equal(newTime, eventEntity.Time);
            Assert.Equal(newLocation, eventEntity.Location);
            Assert.Equal(newAdditionalInfo, eventEntity.AdditionalInfo);
        }

        [Fact]
        public void Event_UpdateDetails_ShouldThrowException_WhenEventTimeInPast()
        {
            var eventEntity = new Event(
                "Original Event",
                DateTime.UtcNow.AddDays(1),
                "Original Location",
                "Original Info"
            );

            var exception = Assert.Throws<ArgumentException>(() =>
                eventEntity.UpdateDetails(
                    "Updated Event",
                    DateTime.UtcNow.AddDays(-1),
                    "Updated Location",
                    "Updated Info"
                )
            );
            Assert.Equal(
                "Event time must be in the future. (Parameter 'eventTime')",
                exception.Message
            );
        }

        [Fact]
        public void IndividualParticipant_Constructor_ShouldCreateParticipantWithCorrectProperties()
        {
            var firstName = "John";
            var lastName = "Doe";
            var personalIdCode = "39001011234";

            var participant = new IndividualParticipant(firstName, lastName, personalIdCode);

            Assert.Equal(firstName, participant.FirstName);
            Assert.Equal(lastName, participant.LastName);
            Assert.Equal(personalIdCode, participant.PersonalIdCode);
        }

        [Fact]
        public void CompanyParticipant_Constructor_ShouldCreateParticipantWithCorrectProperties()
        {
            var legalName = "Test Company Ltd";
            var registryCode = "12345678";
            var numberOfParticipants = 5;

            var participant = new CompanyParticipant(legalName, registryCode, numberOfParticipants);

            Assert.Equal(legalName, participant.LegalName);
            Assert.Equal(registryCode, participant.RegistryCode);
            Assert.Equal(numberOfParticipants, participant.NumberOfParticipants);
        }

        [Fact]
        public void PaymentMethod_Constructor_ShouldCreatePaymentMethodWithCorrectName()
        {
            var name = "Credit Card";

            var paymentMethod = new PaymentMethod(name);

            Assert.Equal(name, paymentMethod.Name);
        }

        [Fact]
        public void EventParticipant_Properties_ShouldBeSettableAndGettable()
        {
            var eventId = Guid.NewGuid();
            var participantId = 1;
            var paymentMethodId = 1;
            var additionalInfo = "Test additional info";

            var eventParticipant = new EventParticipant
            {
                EventId = eventId,
                ParticipantId = participantId,
                PaymentMethodId = paymentMethodId,
                AdditionalInfo = additionalInfo,
            };

            Assert.Equal(eventId, eventParticipant.EventId);
            Assert.Equal(participantId, eventParticipant.ParticipantId);
            Assert.Equal(paymentMethodId, eventParticipant.PaymentMethodId);
            Assert.Equal(additionalInfo, eventParticipant.AdditionalInfo);
        }

        [Fact]
        public void Event_DefaultConstructor_ShouldInitializeWithEmptyValues()
        {
            var eventEntity = new Event();

            Assert.Equal(string.Empty, eventEntity.Name);
            Assert.Equal(string.Empty, eventEntity.Location);
            Assert.Equal(string.Empty, eventEntity.AdditionalInfo);
            Assert.NotNull(eventEntity.Participants);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Event_UpdateDetails_ShouldHandleNullOrEmptyAdditionalInfo(
            string? additionalInfo
        )
        {
            var eventEntity = new Event(
                "Test Event",
                DateTime.UtcNow.AddDays(1),
                "Test Location",
                "Original Info"
            );

            eventEntity.UpdateDetails(
                "Updated Event",
                DateTime.UtcNow.AddDays(2),
                "Updated Location",
                additionalInfo
            );

            Assert.Equal(string.Empty, eventEntity.AdditionalInfo);
        }
    }
}
