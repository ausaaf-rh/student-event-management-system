using StudentEventAPI.Models;

namespace StudentEventAPI.Services
{
    public interface IFeedbackProcessingService
    {
        Task<IEnumerable<Feedback>> GetAllFeedbackAsync();
        Task<Feedback?> GetFeedbackByIdAsync(int feedbackId);
        Task<Feedback> SubmitFeedbackAsync(Feedback feedbackData);
        Task<IEnumerable<Feedback>> GetFeedbackForEventAsync(int eventId);
        Task<IEnumerable<Feedback>> GetFeedbackByStudentAsync(int studentId);
        Task<double> GetAverageRatingForEventAsync(int eventId);
        Task<bool> HasStudentProvidedFeedbackAsync(int eventId, int studentId);
        Task<Dictionary<string, object>> GetEventFeedbackSummaryAsync(int eventId);
    }
}
