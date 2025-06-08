using EventRegistration.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventRegistration.Infrastructure.Repositories
{
    public class EventRepository : IEventRepository
    {
        private readonly EventRegistrationDbContext _context;

        public EventRepository(EventRegistrationDbContext context)
        {
            _context = context;
        }

        public async Task<Event?> GetByIdAsync(Guid id) // Return type changed to nullable Event
        {
            return await _context.Events.Include(e => e.EventParticipants).FirstOrDefaultAsync(e => e.Id == id); // Added Include and changed to FirstOrDefaultAsync
        }

        public async Task<IEnumerable<Event>> GetAllAsync()
        {
            return await _context.Events.ToListAsync();
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

        public async Task DeleteAsync(Guid id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _context.Events.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }

    public class ParticipantRepository : IParticipantRepository
    {
        private readonly EventRegistrationDbContext _context;

        public ParticipantRepository(EventRegistrationDbContext context)
        {
            _context = context;
        }

        public async Task<Participant?> GetByIdAsync(Guid id) // Return type changed to nullable Participant
        {
            return await _context.Participants.FindAsync(id);
        }

        public async Task<IEnumerable<Participant>> GetByEventIdAsync(Guid eventId)
        {
            return await _context.EventParticipants
                .Where(ep => ep.EventId == eventId)
                .Select(ep => ep.Participant!)
                .ToListAsync(); // Added null-forgiving operator as Participant should exist if EventParticipant exists
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

        public async Task DeleteAsync(Guid id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _context.Participants.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }

    public class PaymentMethodRepository : IPaymentMethodRepository
    {
        private readonly EventRegistrationDbContext _context;

        public PaymentMethodRepository(EventRegistrationDbContext context)
        {
            _context = context;
        }

        public async Task<PaymentMethod?> GetByIdAsync(Guid id) // Return type changed to nullable PaymentMethod
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

        public async Task DeleteAsync(Guid id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _context.PaymentMethods.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}
