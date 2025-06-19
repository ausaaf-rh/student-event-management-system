using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentEventAPI.Data;
using StudentEventAPI.Models;
using StudentEventAPI.Services;
using System.ComponentModel.DataAnnotations;

namespace StudentEventAPI.Controllers
{
    [Route("api/gathering-enrollments")]
    [ApiController]
    [Produces("application/json")]
    public class RegistrationsController : ControllerBase
    {
        private readonly AppDbContext _dataRepository;
        private readonly IEventManagementService _eventHandler;
        private readonly IStudentRegistrationService _studentHandler;
        private readonly ILogger<RegistrationsController> _systemLogger;

        public RegistrationsController(
            AppDbContext dataRepository,
            IEventManagementService eventHandler,
            IStudentRegistrationService studentHandler,
            ILogger<RegistrationsController> systemLogger)
        {
            _dataRepository = dataRepository;
            _eventHandler = eventHandler;
            _studentHandler = studentHandler;
            _systemLogger = systemLogger;
        }

        /// <summary>
        /// Retrieves all gathering enrollment records
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Registration>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Registration>>> FetchAllEnrollments()
        {
            try
            {
                var enrollments = await _dataRepository.Registrations
                    .Include(r => r.Event)
                    .Include(r => r.Student)
                    .OrderByDescending(r => r.RegistrationTimestamp)
                    .ToListAsync();

                _systemLogger.LogInformation("Successfully retrieved {Count} enrollment records from database", enrollments.Count);
                return Ok(enrollments);
            }
            catch (Exception ex)
            {
                _systemLogger.LogError(ex, "Failed to retrieve enrollment records from data source");
                return StatusCode(500, "System encountered an issue while fetching enrollment data");
            }
        }

        /// <summary>
        /// Obtains specific enrollment record using its unique identifier
        /// </summary>
        [HttpGet("{enrollmentId:int}")]
        [ProducesResponseType(typeof(Registration), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Registration>> FetchEnrollmentDetails(int enrollmentId)
        {
            try
            {
                var enrollment = await _dataRepository.Registrations
                    .Include(r => r.Event)
                    .Include(r => r.Student)
                    .FirstOrDefaultAsync(r => r.RegistrationId == enrollmentId);

                if (enrollment == null)
                {
                    _systemLogger.LogWarning("Enrollment record with ID {EnrollmentId} could not be located", enrollmentId);
                    return NotFound($"Enrollment record with ID {enrollmentId} could not be located");
                }

                return Ok(enrollment);
            }
            catch (Exception ex)
            {
                _systemLogger.LogError(ex, "System failure while fetching enrollment record {EnrollmentId}", enrollmentId);
                return StatusCode(500, "System encountered an issue while fetching enrollment data");
            }
        }

        /// <summary>
        /// Establishes new gathering enrollment for a learner
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(Registration), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Registration>> EstablishGatheringEnrollment([FromBody] Registration enrollmentData)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Verify gathering exists and has capacity
                var gathering = await _eventHandler.GetEventByIdAsync(enrollmentData.EventId);
                if (gathering == null)
                    return BadRequest("Specified gathering does not exist");

                var hasCapacity = await _eventHandler.HasAvailableSpotsAsync(enrollmentData.EventId);
                if (!hasCapacity)
                    return BadRequest("Selected gathering has reached maximum capacity");

                // Verify learner exists
                var learner = await _studentHandler.GetStudentByIdAsync(enrollmentData.StudentId);
                if (learner == null)
                    return BadRequest("Specified learner profile does not exist");

                // Check for duplicate enrollment
                var existingEnrollment = await _dataRepository.Registrations
                    .AnyAsync(r => r.EventId == enrollmentData.EventId && r.StudentId == enrollmentData.StudentId);

                if (existingEnrollment)
                    return BadRequest("Learner is already enrolled for this gathering");

                enrollmentData.RegistrationTimestamp = DateTime.UtcNow;
                _dataRepository.Registrations.Add(enrollmentData);
                await _dataRepository.SaveChangesAsync();

                var establishedEnrollment = await _dataRepository.Registrations
                    .Include(r => r.Event)
                    .Include(r => r.Student)
                    .FirstOrDefaultAsync(r => r.RegistrationId == enrollmentData.RegistrationId);

                _systemLogger.LogInformation("Successfully established enrollment with ID {EnrollmentId} for learner {LearnerId} in gathering {GatheringId}",
                    establishedEnrollment!.RegistrationId, enrollmentData.StudentId, enrollmentData.EventId);

                return CreatedAtAction(
                    nameof(FetchEnrollmentDetails),
                    new { enrollmentId = establishedEnrollment.RegistrationId },
                    establishedEnrollment);
            }
            catch (Exception ex)
            {
                _systemLogger.LogError(ex, "Failed to establish gathering enrollment");
                return StatusCode(500, "System encountered an issue while processing enrollment request");
            }
        }

        /// <summary>
        /// Cancels a gathering enrollment
        /// </summary>
        [HttpDelete("{enrollmentId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CancelGatheringEnrollment(int enrollmentId)
        {
            try
            {
                var enrollmentToCancel = await _dataRepository.Registrations.FindAsync(enrollmentId);
                if (enrollmentToCancel == null)
                {
                    _systemLogger.LogWarning("Attempted to cancel non-existent enrollment {EnrollmentId}", enrollmentId);
                    return NotFound($"Enrollment record with ID {enrollmentId} could not be located for cancellation");
                }

                _dataRepository.Registrations.Remove(enrollmentToCancel);
                await _dataRepository.SaveChangesAsync();

                _systemLogger.LogInformation("Successfully cancelled enrollment with ID {EnrollmentId}", enrollmentId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _systemLogger.LogError(ex, "System failure while cancelling enrollment {EnrollmentId}", enrollmentId);
                return StatusCode(500, "System encountered an issue while processing cancellation request");
            }
        }

        /// <summary>
        /// Retrieves all enrollments for a specific gathering
        /// </summary>
        [HttpGet("gathering/{gatheringId:int}")]
        [ProducesResponseType(typeof(IEnumerable<Registration>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Registration>>> FetchGatheringEnrollments(int gatheringId)
        {
            try
            {
                var gatheringEnrollments = await _dataRepository.Registrations
                    .Where(r => r.EventId == gatheringId)
                    .Include(r => r.Student)
                    .OrderBy(r => r.RegistrationTimestamp)
                    .ToListAsync();

                _systemLogger.LogInformation("Retrieved {Count} enrollments for gathering {GatheringId}",
                    gatheringEnrollments.Count, gatheringId);

                return Ok(gatheringEnrollments);
            }
            catch (Exception ex)
            {
                _systemLogger.LogError(ex, "Failed to retrieve enrollments for gathering {GatheringId}", gatheringId);
                return StatusCode(500, "System encountered an issue while fetching gathering enrollments");
            }
        }

        /// <summary>
        /// Retrieves all enrollments for a specific learner
        /// </summary>
        [HttpGet("learner/{learnerId:int}")]
        [ProducesResponseType(typeof(IEnumerable<Registration>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Registration>>> FetchLearnerEnrollments(int learnerId)
        {
            try
            {
                var learnerEnrollments = await _dataRepository.Registrations
                    .Where(r => r.StudentId == learnerId)
                    .Include(r => r.Event)
                    .OrderByDescending(r => r.RegistrationTimestamp)
                    .ToListAsync();

                _systemLogger.LogInformation("Retrieved {Count} enrollments for learner {LearnerId}",
                    learnerEnrollments.Count, learnerId);

                return Ok(learnerEnrollments);
            }
            catch (Exception ex)
            {
                _systemLogger.LogError(ex, "Failed to retrieve enrollments for learner {LearnerId}", learnerId);
                return StatusCode(500, "System encountered an issue while fetching learner enrollments");
            }
        }

        /// <summary>
        /// Updates enrollment status for a specific registration
        /// </summary>
        [HttpPatch("{enrollmentId:int}/status")]
        [ProducesResponseType(typeof(Registration), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Registration>> UpdateEnrollmentStatus(int enrollmentId, [FromBody] RegistrationStatus newStatus)
        {
            try
            {
                var enrollment = await _dataRepository.Registrations
                    .Include(r => r.Event)
                    .Include(r => r.Student)
                    .FirstOrDefaultAsync(r => r.RegistrationId == enrollmentId);

                if (enrollment == null)
                {
                    _systemLogger.LogWarning("Attempted to update status for non-existent enrollment {EnrollmentId}", enrollmentId);
                    return NotFound($"Enrollment record with ID {enrollmentId} could not be located");
                }

                enrollment.Status = newStatus;
                await _dataRepository.SaveChangesAsync();

                _systemLogger.LogInformation("Successfully updated enrollment {EnrollmentId} status to {Status}", enrollmentId, newStatus);
                return Ok(enrollment);
            }
            catch (Exception ex)
            {
                _systemLogger.LogError(ex, "System failure while updating enrollment status {EnrollmentId}", enrollmentId);
                return StatusCode(500, "System encountered an issue while updating enrollment status");
            }
        }
    }
}