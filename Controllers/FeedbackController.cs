using Microsoft.AspNetCore.Mvc;
using StudentEventAPI.Models;
using StudentEventAPI.Services;
using System.ComponentModel.DataAnnotations;

namespace StudentEventAPI.Controllers
{
    [Route("api/gathering-feedback")]
    [ApiController]
    [Produces("application/json")]
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackProcessingService _feedbackProcessor;
        private readonly ILogger<FeedbackController> _systemLogger;

        public FeedbackController(IFeedbackProcessingService feedbackProcessor, ILogger<FeedbackController> systemLogger)
        {
            _feedbackProcessor = feedbackProcessor;
            _systemLogger = systemLogger;
        }

        /// <summary>
        /// Retrieves all gathering feedback submissions
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Feedback>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Feedback>>> FetchAllGatheringFeedback()
        {
            try
            {
                var feedbackEntries = await _feedbackProcessor.GetAllFeedbackAsync();
                _systemLogger.LogInformation("Successfully retrieved {Count} feedback entries from database", feedbackEntries.Count());
                return Ok(feedbackEntries);
            }
            catch (Exception ex)
            {
                _systemLogger.LogError(ex, "Failed to retrieve feedback entries from data source");
                return StatusCode(500, "System encountered an issue while fetching feedback data");
            }
        }

        /// <summary>
        /// Obtains specific feedback entry using its unique identifier
        /// </summary>
        [HttpGet("{feedbackId:int}")]
        [ProducesResponseType(typeof(Feedback), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Feedback>> FetchFeedbackDetails(int feedbackId)
        {
            try
            {
                var feedbackEntry = await _feedbackProcessor.GetFeedbackByIdAsync(feedbackId);
                if (feedbackEntry == null)
                {
                    _systemLogger.LogWarning("Feedback entry with ID {FeedbackId} could not be located", feedbackId);
                    return NotFound($"Feedback entry with ID {feedbackId} could not be located");
                }

                return Ok(feedbackEntry);
            }
            catch (Exception ex)
            {
                _systemLogger.LogError(ex, "System failure while fetching feedback entry {FeedbackId}", feedbackId);
                return StatusCode(500, "System encountered an issue while fetching feedback data");
            }
        }

        /// <summary>
        /// Submits new feedback entry for a gathering
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(Feedback), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Feedback>> SubmitGatheringFeedback([FromBody] Feedback feedbackData)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var submittedFeedback = await _feedbackProcessor.SubmitFeedbackAsync(feedbackData);
                _systemLogger.LogInformation("Successfully submitted feedback entry with ID {FeedbackId} for gathering {GatheringId}",
                    submittedFeedback.Id, submittedFeedback.EventId);

                return CreatedAtAction(
                    nameof(FetchFeedbackDetails),
                    new { feedbackId = submittedFeedback.Id },
                    submittedFeedback);
            }
            catch (InvalidOperationException ex)
            {
                _systemLogger.LogWarning(ex, "Invalid operation while submitting feedback");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _systemLogger.LogError(ex, "Failed to submit feedback entry");
                return StatusCode(500, "System encountered an issue while processing feedback submission");
            }
        }

        /// <summary>
        /// Retrieves all feedback entries for a specific gathering
        /// </summary>
        [HttpGet("gathering/{gatheringId:int}")]
        [ProducesResponseType(typeof(IEnumerable<Feedback>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Feedback>>> FetchGatheringFeedbackEntries(int gatheringId)
        {
            try
            {
                var gatheringFeedback = await _feedbackProcessor.GetFeedbackForEventAsync(gatheringId);
                _systemLogger.LogInformation("Retrieved {Count} feedback entries for gathering {GatheringId}",
                    gatheringFeedback.Count(), gatheringId);

                return Ok(gatheringFeedback);
            }
            catch (Exception ex)
            {
                _systemLogger.LogError(ex, "Failed to retrieve feedback for gathering {GatheringId}", gatheringId);
                return StatusCode(500, "System encountered an issue while fetching gathering feedback");
            }
        }

        /// <summary>
        /// Retrieves all feedback entries submitted by a specific learner
        /// </summary>
        [HttpGet("learner/{learnerId:int}")]
        [ProducesResponseType(typeof(IEnumerable<Feedback>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Feedback>>> FetchLearnerFeedbackEntries(int learnerId)
        {
            try
            {
                var learnerFeedback = await _feedbackProcessor.GetFeedbackByStudentAsync(learnerId);
                _systemLogger.LogInformation("Retrieved {Count} feedback entries submitted by learner {LearnerId}",
                    learnerFeedback.Count(), learnerId);

                return Ok(learnerFeedback);
            }
            catch (Exception ex)
            {
                _systemLogger.LogError(ex, "Failed to retrieve feedback for learner {LearnerId}", learnerId);
                return StatusCode(500, "System encountered an issue while fetching learner feedback");
            }
        }

        /// <summary>
        /// Calculates average rating for a specific gathering
        /// </summary>
        [HttpGet("gathering/{gatheringId:int}/average-rating")]
        [ProducesResponseType(typeof(double), StatusCodes.Status200OK)]
        public async Task<ActionResult<double>> CalculateGatheringAverageRating(int gatheringId)
        {
            try
            {
                var averageRating = await _feedbackProcessor.GetAverageRatingForEventAsync(gatheringId);
                _systemLogger.LogInformation("Calculated average rating {Rating} for gathering {GatheringId}",
                    averageRating, gatheringId);

                return Ok(averageRating);
            }
            catch (Exception ex)
            {
                _systemLogger.LogError(ex, "Failed to calculate average rating for gathering {GatheringId}", gatheringId);
                return StatusCode(500, "System encountered an issue while calculating rating");
            }
        }

        /// <summary>
        /// Verifies if a learner has provided feedback for a specific gathering
        /// </summary>
        [HttpGet("gathering/{gatheringId:int}/learner/{learnerId:int}/submission-status")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<ActionResult<bool>> VerifyLearnerFeedbackSubmission(int gatheringId, int learnerId)
        {
            try
            {
                var hasSubmitted = await _feedbackProcessor.HasStudentProvidedFeedbackAsync(gatheringId, learnerId);
                return Ok(hasSubmitted);
            }
            catch (Exception ex)
            {
                _systemLogger.LogError(ex, "Failed to verify feedback submission status for learner {LearnerId} and gathering {GatheringId}",
                    learnerId, gatheringId);
                return StatusCode(500, "System encountered an issue while checking submission status");
            }
        }

        /// <summary>
        /// Generates comprehensive feedback summary for a gathering
        /// </summary>
        [HttpGet("gathering/{gatheringId:int}/summary")]
        [ProducesResponseType(typeof(Dictionary<string, object>), StatusCodes.Status200OK)]
        public async Task<ActionResult<Dictionary<string, object>>> GenerateGatheringFeedbackSummary(int gatheringId)
        {
            try
            {
                var feedbackSummary = await _feedbackProcessor.GetEventFeedbackSummaryAsync(gatheringId);
                _systemLogger.LogInformation("Generated comprehensive feedback summary for gathering {GatheringId}", gatheringId);

                return Ok(feedbackSummary);
            }
            catch (Exception ex)
            {
                _systemLogger.LogError(ex, "Failed to generate feedback summary for gathering {GatheringId}", gatheringId);
                return StatusCode(500, "System encountered an issue while generating summary");
            }
        }
    }
}