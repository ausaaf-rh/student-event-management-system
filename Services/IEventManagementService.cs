using StudentEventAPI.Models;

namespace StudentEventAPI.Services
{   
    public interface IEventManagementService
    {
        Task<IEnumerable<Event>> GetAllEventsAsync();
        Task<Event?> GetEventByIdAsync(int eventId);
        Task<Event> CreateEventAsync(Event eventData);
        Task<Event?> UpdateEventAsync(int eventId, Event eventData);
        Task<bool> DeleteEventAsync(int eventId);
        Task<IEnumerable<Event>> SearchEventsAsync(string searchQuery);
        Task<IEnumerable<Event>> GetEventsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Event>> GetEventsByCategoryAsync(EventCategory category);
        Task<bool> HasAvailableSpotsAsync(int eventId);
        Task<int> GetAvailableCapacityAsync(int eventId);
    }
}
