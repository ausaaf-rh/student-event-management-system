using Microsoft.AspNetCore.Mvc;
using StudentEventAPI.Models;
using StudentEventAPI.Services;
using System.ComponentModel.DataAnnotations;

namespace StudentEventAPI.Controllers
{
    [Route("api/university-gatherings")]
    [ApiController]
    [Produces("application/json")]
    public class EventsController : ControllerBase
    {
        private readonly IEventManagementService _eventHandler;
        private readonly ILogger<EventsController> _systemLogger;

        public EventsController(IEventManagementService eventHandler, ILogger<EventsController> systemLogger)
        {
            _eventHandler = eventHandler;
            _systemLogger = systemLogger;
        }
        
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Event>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Event>>> FetchAllUniversityGatherings()
        {
            try
            {
                var gatherings = await _eventHandler.GetAllEventsAsync();
                _systemLogger.LogInformation("Successfully fetched {Count} gatherings from database", gatherings.Count());
                return Ok(gatherings);
            }
            catch (Exception ex)
            {
                _systemLogger.LogError(ex, "Failed to retrieve gatherings from data source");
                return StatusCode(500, "System encountered an issue while fetching data");
            }
        }

        /// <summary>
        /// Obtains detailed information for a specific gathering using its unique identifier
        /// </summary>
        [HttpGet("{gatheringId:int}")]
        [ProducesResponseType(typeof(Event), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Event>> FetchGatheringInformation(int gatheringId)
        {
            try
            {
                var gatheringDetails = await _eventHandler.GetEventByIdAsync(gatheringId);
                if (gatheringDetails == null)
                {
                    _systemLogger.LogWarning("University gathering with ID {GatheringId} could not be located", gatheringId);
                    return NotFound($"University gathering with ID {gatheringId} could not be located");
                }

                return Ok(gatheringDetails);
            }
            catch (Exception ex)
            {
                _systemLogger.LogError(ex, "System failure while fetching gathering {GatheringId}", gatheringId);
                return StatusCode(500, "System encountered an issue while fetching data");
            }
        }

        /// <summary>
        /// Establishes a new university gathering entry
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(Event), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Event>> EstablishNewGathering([FromBody] Event gatheringData)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var establishedGathering = await _eventHandler.CreateEventAsync(gatheringData);
                _systemLogger.LogInformation("Successfully established new gathering with ID {GatheringId}", establishedGathering.Id);
                
                return CreatedAtAction(
                    nameof(FetchGatheringInformation), 
                    new { gatheringId = establishedGathering.Id }, 
                    establishedGathering);
            }
            catch (Exception ex)
            {
                _systemLogger.LogError(ex, "Failed to establish new gathering in system");
                return StatusCode(500, "System encountered an issue while processing data");
            }
        }

        /// <summary>
        /// Modifies details of an existing university gathering
        /// </summary>
        [HttpPut("{gatheringId:int}")]
        [ProducesResponseType(typeof(Event), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Event>> ReviseGatheringDetails(int gatheringId, [FromBody] Event gatheringData)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var revisedGathering = await _eventHandler.UpdateEventAsync(gatheringId, gatheringData);
                if (revisedGathering == null)
                {
                    _systemLogger.LogWarning("Attempted to modify non-existent gathering {GatheringId}", gatheringId);
                    return NotFound($"University gathering with ID {gatheringId} could not be located for modification");
                }

                _systemLogger.LogInformation("Successfully revised gathering with ID {GatheringId}", gatheringId);
                return Ok(revisedGathering);
            }
            catch (Exception ex)
            {
                _systemLogger.LogError(ex, "System failure while revising gathering {GatheringId}", gatheringId);
                return StatusCode(500, "System encountered an issue while processing modifications");
            }
        }

        /// <summary>
        /// Eliminates a gathering from the university system
        /// </summary>
        [HttpDelete("{gatheringId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> EliminateGathering(int gatheringId)
        {
            try
            {
                var wasEliminated = await _eventHandler.DeleteEventAsync(gatheringId);
                if (!wasEliminated)
                {
                    _systemLogger.LogWarning("Attempted to eliminate non-existent gathering {GatheringId}", gatheringId);
                    return NotFound($"University gathering with ID {gatheringId} could not be located for elimination");
                }

                _systemLogger.LogInformation("Successfully eliminated gathering with ID {GatheringId}", gatheringId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _systemLogger.LogError(ex, "System failure while eliminating gathering {GatheringId}", gatheringId);
                return StatusCode(500, "System encountered an issue while processing elimination request");
            }
        }

        /// <summary>
        /// Conducts search operations across gatherings using specified criteria
        /// </summary>
        [HttpGet("search")]
        [ProducesResponseType(typeof(IEnumerable<Event>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Event>>> ConductGatheringSearch(
            [FromQuery, Required] string searchCriteria)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchCriteria))
                    return BadRequest("Search criteria must contain valid text");

                var searchResults = await _eventHandler.SearchEventsAsync(searchCriteria);
                _systemLogger.LogInformation("Search operation using '{SearchCriteria}' yielded {Count} results", searchCriteria, searchResults.Count());
                
                return Ok(searchResults);
            }
            catch (Exception ex)
            {
                _systemLogger.LogError(ex, "Search operation failed with criteria '{SearchCriteria}'", searchCriteria);
                return StatusCode(500, "System encountered an issue while conducting search");
            }
        }

        /// <summary>
        /// Retrieves gatherings organized by classification type
        /// </summary>
        [HttpGet("classifications/{classification}")]
        [ProducesResponseType(typeof(IEnumerable<Event>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Event>>> FetchGatheringsByClassification(EventCategory classification)
        {
            try
            {
                var classificationGatherings = await _eventHandler.GetEventsByCategoryAsync(classification);
                _systemLogger.LogInformation("Retrieved {Count} gatherings for classification {Classification}", classificationGatherings.Count(), classification);
                
                return Ok(classificationGatherings);
            }
            catch (Exception ex)
            {
                _systemLogger.LogError(ex, "Failed to retrieve gatherings for classification {Classification}", classification);
                return StatusCode(500, "System encountered an issue while fetching classified gatherings");
            }
        }

        /// <summary>
        /// Obtains gatherings scheduled within specified time frame
        /// </summary>
        [HttpGet("timeframe")]
        [ProducesResponseType(typeof(IEnumerable<Event>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Event>>> FetchGatheringsByTimeframe(
            [FromQuery, Required] DateTime beginDate,
            [FromQuery, Required] DateTime endDate)
        {
            try
            {
                if (beginDate > endDate)
                    return BadRequest("Begin date must precede end date in timeframe");

                var timeframeGatherings = await _eventHandler.GetEventsByDateRangeAsync(beginDate, endDate);
                _systemLogger.LogInformation("Retrieved {Count} gatherings between {BeginDate} and {EndDate}", 
                    timeframeGatherings.Count(), beginDate, endDate);
                
                return Ok(timeframeGatherings);
            }
            catch (Exception ex)
            {
                _systemLogger.LogError(ex, "Failed to retrieve gatherings for timeframe {BeginDate} to {EndDate}", 
                    beginDate, endDate);
                return StatusCode(500, "System encountered an issue while fetching timeframe data");
            }
        }

        /// <summary>
        /// Verifies participant capacity status for a specific gathering
        /// </summary>
        [HttpGet("{gatheringId:int}/capacity-status")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<object>> VerifyGatheringCapacityStatus(int gatheringId)
        {
            try
            {
                var gatheringExists = await _eventHandler.GetEventByIdAsync(gatheringId);
                if (gatheringExists == null)
                    return NotFound($"University gathering with ID {gatheringId} could not be located");

                var hasOpenings = await _eventHandler.HasAvailableSpotsAsync(gatheringId);
                var remainingCapacity = await _eventHandler.GetAvailableCapacityAsync(gatheringId);

                return Ok(new
                {
                    GatheringId = gatheringId,
                    HasOpenSpots = hasOpenings,
                    RemainingCapacity = remainingCapacity,
                    TotalCapacity = gatheringExists.MaxCapacity,
                    CurrentParticipants = gatheringExists.CurrentParticipantCount
                });
            }
            catch (Exception ex)
            {
                _systemLogger.LogError(ex, "Failed to verify capacity status for gathering {GatheringId}", gatheringId);
                return StatusCode(500, "System encountered an issue while checking capacity status");
            }
        }
    }
}
