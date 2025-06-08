using EventRegistration.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// The namespace should be EventRegistration.Infrastructure, not Repositories
namespace EventRegistration.Infrastructure
{
    public class EventRepository : IEventRepository
    {
        private readonly EventRegistrationDbContext _context;

        public EventRepository(EventRegistrationDbContext context)
        {
            _context = context;
        }

        public async Task<Event?> GetByIdAsync(Guid id) 
        {
            // Corrected the Include to use the 'Participants' navigation property
            return await _context.Events.Include(e => e.Participants).FirstOrDefaultAsync(e => e.Id == id); 
        }

        public async Task<IEnumerable<Event>> GetAllAsync()
        {
            // Corrected the Include to use the 'Participants' navigation property
            return await _context.Events.Include(e => e.Participants).ToListAsync();
        }

        public async Task AddAsync(Event entity)
        {
            await _context.Events.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Event entity)
        {
            _context.Events.Update(entity);
            await _context.SaveChangesAsync();
        }

        // FIX: Implemented RemoveAsync to match the interface
        public async Task RemoveAsync(Event entity)
        {
            _context.Events.Remove(entity);
            await _context.SaveChangesAsync();
        }
        
        // FIX: Added missing GetEventWithParticipantsAsync method
        public async Task<Event?> GetEventWithParticipantsAsync(Guid eventId)
        {
            return await _context.Events
                .Include(e => e.Participants)
                .ThenInclude(ep => ep.Participant)
                .FirstOrDefaultAsync(e => e.Id == eventId);
        }

        // FIX: Added missing GetUpcomingEvents method
        public async Task<IEnumerable<Event>> GetUpcomingEvents()
        {
            return await _context.Events
                .Where(e => e.Time > DateTime.UtcNow)
                .OrderBy(e => e.Time)
                .ToListAsync();
        }
    }

   public class ParticipantRepository : IParticipantRepository
    {
        private readonly EventRegistrationDbContext _context;

        public ParticipantRepository(EventRegistrationDbContext context)
        {
            _context = context;
        }

        // FIX: Changed parameter type from Guid to int to match the interface.
        public async Task<Participant?> GetByIdAsync(int id)
        {
            return await _context.Participants.FindAsync(id);
        }

        public async Task<IEnumerable<Participant>> GetAllAsync()
        {
            return await _context.Participants.ToListAsync();
        }

        public async Task<IEnumerable<Participant>> GetByEventIdAsync(Guid eventId)
        {
            return await _context.EventParticipants
                .Where(ep => ep.EventId == eventId)
                .Select(ep => ep.Participant!)
                .ToListAsync();
        }

        public async Task AddAsync(Participant entity)
        {
            await _context.Participants.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Participant entity)
        {
            _context.Participants.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveAsync(Participant entity)
        {
            _context.Participants.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<IndividualParticipant?> GetIndividualByPersonalIdCodeAsync(string personalIdCode)
        {
            return await _context.Participants
                .OfType<IndividualParticipant>()
                .FirstOrDefaultAsync(p => p.PersonalIdCode == personalIdCode);
        }

        public async Task<CompanyParticipant?> GetCompanyByRegistryCodeAsync(string registryCode)
        {
            return await _context.Participants
                .OfType<CompanyParticipant>()
                .FirstOrDefaultAsync(p => p.RegistryCode == registryCode);
        }
    }

    public class PaymentMethodRepository : IPaymentMethodRepository
    {
        private readonly EventRegistrationDbContext _context;

        public PaymentMethodRepository(EventRegistrationDbContext context)
        {
            _context = context;
        }

        // FIX: Changed parameter type from Guid to int to match the interface.
        public async Task<PaymentMethod?> GetByIdAsync(int id)
        {
            return await _context.PaymentMethods.FindAsync(id);
        }

        public async Task<IEnumerable<PaymentMethod>> GetAllAsync()
        {
            return await _context.PaymentMethods.ToListAsync();
        }

        public async Task AddAsync(PaymentMethod entity)
        {
            await _context.PaymentMethods.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(PaymentMethod entity)
        {
            _context.PaymentMethods.Update(entity);
            await _context.SaveChangesAsync();
        }

        // FIX: Changed parameter type from Guid to int to match the interface.
        public async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _context.PaymentMethods.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }

    public class EventParticipantRepository : IEventParticipantRepository
    {
        private readonly EventRegistrationDbContext _context;

        public EventParticipantRepository(EventRegistrationDbContext context)
        {
            _context = context;
        }
        
        // This method is not in the interface and can be removed or kept for internal use.
        // If you need it, ensure its usage is consistent.
        // public async Task<EventParticipant?> GetByIdAsync(Guid id)
        // {
        //     return await _context.EventParticipants.FindAsync(id);
        // }

        public async Task<IEnumerable<EventParticipant>> GetByEventIdAsync(Guid eventId)
        {
            return await _context.EventParticipants
                .Where(ep => ep.EventId == eventId)
                .Include(ep => ep.Participant)
                .Include(ep => ep.PaymentMethod)
                .ToListAsync();
        }

        public async Task AddAsync(EventParticipant entity)
        {
            await _context.EventParticipants.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(EventParticipant entity)
        {
            _context.EventParticipants.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(EventParticipant entity)
        {
            if (entity != null)
            {
                _context.EventParticipants.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        // FIX: Changed participantId from Guid to int to match the interface.
        public async Task<EventParticipant?> GetByEventAndParticipantAsync(Guid eventId, int participantId)
        {
            return await _context.EventParticipants
                .Include(ep => ep.Participant)
                .Include(ep => ep.PaymentMethod)
                .FirstOrDefaultAsync(ep => ep.EventId.Equals(eventId) && ep.ParticipantId.Equals(participantId));
        }
    }
}