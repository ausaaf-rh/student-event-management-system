using Microsoft.EntityFrameworkCore;
using StudentEventAPI.Data;
using StudentEventAPI.Models;

namespace StudentEventAPI.Services
{
    public class EventManagementService : IEventManagementService
    {
        private readonly AppDbContext _dataRepository;

        public EventManagementService(AppDbContext dataRepository)
        {
            _dataRepository = dataRepository;
        }

        public async Task<IEnumerable<Event>> GetAllEventsAsync()
        {
            return await _dataRepository.Events
                .Include(e => e.Registrations)
                .ThenInclude(r => r.Student)
                .OrderBy(e => e.Date)
                .ToListAsync();
        }

        public async Task<Event?> GetEventByIdAsync(int eventId)
        {
            return await _dataRepository.Events
                .Include(e => e.Registrations)
                .ThenInclude(r => r.Student)
                .Include(e => e.Feedbacks)
                .FirstOrDefaultAsync(e => e.Id == eventId);
        }

        public async Task<Event> CreateEventAsync(Event eventData)
        {
            eventData.CreatedAt = DateTime.UtcNow;
            eventData.ModifiedAt = DateTime.UtcNow;

            _dataRepository.Events.Add(eventData);
            await _dataRepository.SaveChangesAsync();
            return eventData;
        }

        public async Task<Event?> UpdateEventAsync(int eventId, Event eventData)
        {
            var existingGathering = await _dataRepository.Events.FindAsync(eventId);
            if (existingGathering == null)
                return null;

            existingGathering.Name = eventData.Name;
            existingGathering.Description = eventData.Description;
            existingGathering.Venue = eventData.Venue;
            existingGathering.Date = eventData.Date;
            existingGathering.MaxCapacity = eventData.MaxCapacity;
            existingGathering.RegistrationDeadline = eventData.RegistrationDeadline;
            existingGathering.Category = eventData.Category;
            existingGathering.ModifiedAt = DateTime.UtcNow;

            await _dataRepository.SaveChangesAsync();
            return existingGathering;
        }

        public async Task<bool> DeleteEventAsync(int eventId)
        {
            var gatheringToRemove = await _dataRepository.Events.FindAsync(eventId);
            if (gatheringToRemove == null)
                return false;

            _dataRepository.Events.Remove(gatheringToRemove);
            await _dataRepository.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Event>> SearchEventsAsync(string searchQuery)
        {
            if (string.IsNullOrWhiteSpace(searchQuery))
                return await GetAllEventsAsync();

            var searchCriteria = searchQuery.ToLower().Trim();
            
            return await _dataRepository.Events
                .Where(e => e.Name.ToLower().Contains(searchCriteria) ||
                           e.Description.ToLower().Contains(searchCriteria) ||
                           e.Venue.ToLower().Contains(searchCriteria))
                .Include(e => e.Registrations)
                .OrderBy(e => e.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<Event>> GetEventsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dataRepository.Events
                .Where(e => e.Date >= startDate && e.Date <= endDate)
                .Include(e => e.Registrations)
                .OrderBy(e => e.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<Event>> GetEventsByCategoryAsync(EventCategory category)
        {
            return await _dataRepository.Events
                .Where(e => e.Category == category)
                .Include(e => e.Registrations)
                .OrderBy(e => e.Date)
                .ToListAsync();
        }

        public async Task<bool> HasAvailableSpotsAsync(int eventId)
        {
            var gatheringEntity = await _dataRepository.Events
                .Include(e => e.Registrations)
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (gatheringEntity == null)
                return false;

            return gatheringEntity.CurrentParticipantCount < gatheringEntity.MaxCapacity;
        }

        public async Task<int> GetAvailableCapacityAsync(int eventId)
        {
            var gatheringEntity = await _dataRepository.Events
                .Include(e => e.Registrations)
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (gatheringEntity == null)
                return 0;

            return Math.Max(0, gatheringEntity.MaxCapacity - gatheringEntity.CurrentParticipantCount);
        }
    }
}
