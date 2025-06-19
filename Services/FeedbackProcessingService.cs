using Microsoft.EntityFrameworkCore;
using StudentEventAPI.Data;
using StudentEventAPI.Models;

namespace StudentEventAPI.Services
{
    public class FeedbackProcessingService : IFeedbackProcessingService
    {
        private readonly AppDbContext _databaseContext;

        public FeedbackProcessingService(AppDbContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public async Task<IEnumerable<Feedback>> GetAllFeedbackAsync()
        {
            return await _databaseContext.Feedbacks
                .Include(f => f.Event)
                .Include(f => f.Student)
                .OrderByDescending(f => f.SubmittedAt)
                .ToListAsync();
        }

        public async Task<Feedback?> GetFeedbackByIdAsync(int feedbackId)
        {
            return await _databaseContext.Feedbacks
                .Include(f => f.Event)
                .Include(f => f.Student)
                .FirstOrDefaultAsync(f => f.Id == feedbackId);
        }

        public async Task<Feedback> SubmitFeedbackAsync(Feedback feedbackData)
        {
            // Check if student has already provided feedback for this event
            var existingFeedback = await _databaseContext.Feedbacks
                .FirstOrDefaultAsync(f => f.EventId == feedbackData.EventId && 
                                        f.StudentId == feedbackData.StudentId);

            if (existingFeedback != null)
                throw new InvalidOperationException("Student has already provided feedback for this event.");

            // Verify student is registered for the event
            var isRegistered = await _databaseContext.Registrations
                .AnyAsync(r => r.EventId == feedbackData.EventId && 
                              r.StudentId == feedbackData.StudentId);

            if (!isRegistered)
                throw new InvalidOperationException("Student must be registered for the event to provide feedback.");

            feedbackData.SubmittedAt = DateTime.UtcNow;
            _databaseContext.Feedbacks.Add(feedbackData);
            await _databaseContext.SaveChangesAsync();
            
            return feedbackData;
        }

        public async Task<IEnumerable<Feedback>> GetFeedbackForEventAsync(int eventId)
        {
            return await _databaseContext.Feedbacks
                .Where(f => f.EventId == eventId)
                .Include(f => f.Student)
                .OrderByDescending(f => f.SubmittedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Feedback>> GetFeedbackByStudentAsync(int studentId)
        {
            return await _databaseContext.Feedbacks
                .Where(f => f.StudentId == studentId)
                .Include(f => f.Event)
                .OrderByDescending(f => f.SubmittedAt)
                .ToListAsync();
        }

        public async Task<double> GetAverageRatingForEventAsync(int eventId)
        {
            var feedbacks = await _databaseContext.Feedbacks
                .Where(f => f.EventId == eventId)
                .ToListAsync();

            if (!feedbacks.Any())
                return 0.0;

            return feedbacks.Average(f => f.Rating);
        }

        public async Task<bool> HasStudentProvidedFeedbackAsync(int eventId, int studentId)
        {
            return await _databaseContext.Feedbacks
                .AnyAsync(f => f.EventId == eventId && f.StudentId == studentId);
        }

        public async Task<Dictionary<string, object>> GetEventFeedbackSummaryAsync(int eventId)
        {
            var feedbacks = await _databaseContext.Feedbacks
                .Where(f => f.EventId == eventId)
                .ToListAsync();

            if (!feedbacks.Any())
            {
                return new Dictionary<string, object>
                {
                    ["TotalFeedbacks"] = 0,
                    ["AverageRating"] = 0.0,
                    ["AverageOrganizationRating"] = 0.0,
                    ["AverageContentRating"] = 0.0,
                    ["AverageVenueRating"] = 0.0,
                    ["RecommendationPercentage"] = 0.0,
                    ["RatingDistribution"] = new Dictionary<int, int>()
                };
            }

            var ratingDistribution = feedbacks
                .GroupBy(f => f.Rating)
                .ToDictionary(g => g.Key, g => g.Count());

            var recommendationCount = feedbacks.Count(f => f.WouldRecommend);

            return new Dictionary<string, object>
            {
                ["TotalFeedbacks"] = feedbacks.Count,
                ["AverageRating"] = Math.Round(feedbacks.Average(f => f.Rating), 2),
                ["AverageOrganizationRating"] = Math.Round(feedbacks.Average(f => f.OrganizationRating), 2),
                ["AverageContentRating"] = Math.Round(feedbacks.Average(f => f.ContentRating), 2),
                ["AverageVenueRating"] = Math.Round(feedbacks.Average(f => f.VenueRating), 2),
                ["RecommendationPercentage"] = Math.Round((double)recommendationCount / feedbacks.Count * 100, 2),
                ["RatingDistribution"] = ratingDistribution,
                ["OverallScore"] = Math.Round(feedbacks.Average(f => f.AverageScore), 2)
            };
        }
    }
}
