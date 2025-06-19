using Microsoft.AspNetCore.Mvc;
using StudentEventAPI.Models;
using StudentEventAPI.Services;
using System.ComponentModel.DataAnnotations;

namespace StudentEventAPI.Controllers
{
    [Route("api/learner-profiles")]
    [ApiController]
    [Produces("application/json")]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentRegistrationService _learnerHandler;
        private readonly ILogger<StudentsController> _systemLogger;

        public StudentsController(IStudentRegistrationService learnerHandler, ILogger<StudentsController> systemLogger)
        {
            _learnerHandler = learnerHandler;
            _systemLogger = systemLogger;
        }

        /// <summary>
        /// Fetches complete collection of learner profiles
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Student>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Student>>> FetchAllLearnerProfiles()
        {
            try
            {
                var learners = await _learnerHandler.GetAllStudentsAsync();
                _systemLogger.LogInformation("Successfully fetched {Count} learner profiles from database", learners.Count());
                return Ok(learners);
            }
            catch (Exception ex)
            {
                _systemLogger.LogError(ex, "Failed to retrieve learner profiles from data source");
                return StatusCode(500, "System encountered an issue while fetching learner data");
            }
        }

        /// <summary>
        /// Obtains detailed information for a specific learner using unique identifier
        /// </summary>
        [HttpGet("{learnerId:int}")]
        [ProducesResponseType(typeof(Student), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Student>> FetchLearnerProfileInformation(int learnerId)
        {
            try
            {
                var learnerProfile = await _learnerHandler.GetStudentByIdAsync(learnerId);
                if (learnerProfile == null)
                {
                    _systemLogger.LogWarning("Learner profile with ID {LearnerId} could not be located", learnerId);
                    return NotFound($"Learner profile with ID {learnerId} could not be located");
                }

                return Ok(learnerProfile);
            }
            catch (Exception ex)
            {
                _systemLogger.LogError(ex, "System failure while fetching learner profile {LearnerId}", learnerId);
                return StatusCode(500, "System encountered an issue while fetching learner data");
            }
        }

        /// <summary>
        /// Establishes a new learner profile in the system
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(Student), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Student>> EstablishNewLearnerProfile([FromBody] Student learnerData)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var establishedLearner = await _learnerHandler.CreateStudentProfileAsync(learnerData);
                _systemLogger.LogInformation("Successfully established new learner profile with ID {LearnerId} for {Email}",
                    establishedLearner.Id, establishedLearner.Email);
                
                return CreatedAtAction(
                    nameof(FetchLearnerProfileInformation),
                    new { learnerId = establishedLearner.Id },
                    establishedLearner);
            }
            catch (InvalidOperationException ex)
            {
                _systemLogger.LogWarning(ex, "Invalid operation while establishing learner profile");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _systemLogger.LogError(ex, "Failed to establish new learner profile");
                return StatusCode(500, "System encountered an issue while processing learner data");
            }
        }

        /// <summary>
        /// Modifies details of an existing learner profile
        /// </summary>
        [HttpPut("{learnerId:int}")]
        [ProducesResponseType(typeof(Student), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Student>> ReviseLearnerProfileDetails(int learnerId, [FromBody] Student learnerData)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var revisedLearner = await _learnerHandler.UpdateStudentProfileAsync(learnerId, learnerData);
                if (revisedLearner == null)
                {
                    _systemLogger.LogWarning("Attempted to modify non-existent learner profile {LearnerId}", learnerId);
                    return NotFound($"Learner profile with ID {learnerId} could not be located for modification");
                }

                _systemLogger.LogInformation("Successfully revised learner profile with ID {LearnerId}", learnerId);
                return Ok(revisedLearner);
            }
            catch (InvalidOperationException ex)
            {
                _systemLogger.LogWarning(ex, "Invalid operation while revising learner profile {LearnerId}", learnerId);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _systemLogger.LogError(ex, "System failure while revising learner profile {LearnerId}", learnerId);
                return StatusCode(500, "System encountered an issue while processing modifications");
            }
        }

        /// <summary>
        /// Eliminates a learner profile from the system
        /// </summary>
        [HttpDelete("{learnerId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> EliminateLearnerProfile(int learnerId)
        {
            try
            {
                var wasEliminated = await _learnerHandler.DeleteStudentProfileAsync(learnerId);
                if (!wasEliminated)
                {
                    _systemLogger.LogWarning("Attempted to eliminate non-existent learner profile {LearnerId}", learnerId);
                    return NotFound($"Learner profile with ID {learnerId} could not be located for elimination");
                }

                _systemLogger.LogInformation("Successfully eliminated learner profile with ID {LearnerId}", learnerId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _systemLogger.LogError(ex, "System failure while eliminating learner profile {LearnerId}", learnerId);
                return StatusCode(500, "System encountered an issue while processing elimination request");
            }
        }

        /// <summary>
        /// Conducts search operations across learner profiles using specified criteria
        /// </summary>
        [HttpGet("search")]
        [ProducesResponseType(typeof(IEnumerable<Student>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Student>>> ConductLearnerSearch(
            [FromQuery, Required] string searchCriteria)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchCriteria))
                    return BadRequest("Search criteria must contain valid text");

                var searchResults = await _learnerHandler.SearchStudentsAsync(searchCriteria);
                _systemLogger.LogInformation("Search operation using '{SearchCriteria}' yielded {Count} learner profiles",
                    searchCriteria, searchResults.Count());
                
                return Ok(searchResults);
            }
            catch (Exception ex)
            {
                _systemLogger.LogError(ex, "Search operation failed with criteria '{SearchCriteria}'", searchCriteria);
                return StatusCode(500, "System encountered an issue while conducting search");
            }
        }

        /// <summary>
        /// Locates learner profile using electronic mail address
        /// </summary>
        [HttpGet("by-email/{email}")]
        [ProducesResponseType(typeof(Student), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Student>> LocateLearnerByEmail(string email)
        {
            try
            {
                var learnerProfile = await _learnerHandler.GetStudentByEmailAsync(email);
                if (learnerProfile == null)
                {
                    _systemLogger.LogInformation("No learner profile located for email {Email}", email);
                    return NotFound($"No learner profile found for email {email}");
                }

                return Ok(learnerProfile);
            }
            catch (Exception ex)
            {
                _systemLogger.LogError(ex, "System failure while locating learner profile by email {Email}", email);
                return StatusCode(500, "System encountered an issue while processing email lookup");
            }
        }

        /// <summary>
        /// Retrieves learners organized by academic faculty
        /// </summary>
        [HttpGet("by-faculty/{faculty}")]
        [ProducesResponseType(typeof(IEnumerable<Student>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Student>>> FetchLearnersByFaculty(string faculty)
        {
            try
            {
                var facultyLearners = await _learnerHandler.GetStudentsByDepartmentAsync(faculty);
                _systemLogger.LogInformation("Retrieved {Count} learners from faculty {Faculty}",
                    facultyLearners.Count(), faculty);
                
                return Ok(facultyLearners);
            }
            catch (Exception ex)
            {
                _systemLogger.LogError(ex, "Failed to retrieve learners from faculty {Faculty}", faculty);
                return StatusCode(500, "System encountered an issue while fetching faculty data");
            }
        }

        /// <summary>
        /// Verifies uniqueness of electronic mail address in system
        /// </summary>
        [HttpGet("email-validation")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<ActionResult<object>> VerifyEmailUniqueness(
            [FromQuery, Required] string email,
            [FromQuery] int? excludeLearnerId = null)
        {
            try
            {
                var isUnique = await _learnerHandler.IsEmailUniqueAsync(email, excludeLearnerId);
                return Ok(new
                {
                    Email = email,
                    IsUnique = isUnique,
                    Message = isUnique ? "Email address is available" : "Email address is already in use"
                });
            }
            catch (Exception ex)
            {
                _systemLogger.LogError(ex, "Failed to verify email uniqueness {Email}", email);
                return StatusCode(500, "System encountered an issue while validating email");
            }
        }
    }
}
