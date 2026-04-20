using ApplicationTracker.Common.DTOs;
using ApplicationTracker.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ApplicationTracker.API.Controllers;

/// <summary>
/// Controller für Application CRUD Operationen
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ApplicationsController : ControllerBase
{
    private readonly IApplicationService _applicationService;
    private readonly ILogger<ApplicationsController> _logger;

    public ApplicationsController(IApplicationService applicationService, ILogger<ApplicationsController> logger)
    {
        _applicationService = applicationService;
        _logger = logger;
    }

    // ==================== APPLICATIONS ====================

    /// <summary>
    /// Holt alle Bewerbungen des aktuellen Users
    /// </summary>
    /// <returns>Liste aller Bewerbungen</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<ApplicationDto>>> GetApplications()
    {
        var userId = GetUserIdFromToken();
        if (userId == 0)
            return Unauthorized();

        var applications = await _applicationService.GetApplicationsAsync(userId);
        return Ok(applications);
    }

    /// <summary>
    /// Holt eine einzelne Bewerbung
    /// </summary>
    /// <param name="id">Application ID</param>
    /// <returns>Application Details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApplicationDto>> GetApplication(int id)
    {
        var userId = GetUserIdFromToken();
        if (userId == 0)
            return Unauthorized();

        var application = await _applicationService.GetApplicationAsync(id, userId);
        if (application == null)
            return NotFound();

        return Ok(application);
    }

    /// <summary>
    /// Erstellt eine neue Bewerbung
    /// </summary>
    /// <param name="dto">Bewerbungsdaten</param>
    /// <returns>Erstellte Bewerbung</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApplicationDto>> CreateApplication([FromBody] CreateApplicationDto dto)
    {
        var userId = GetUserIdFromToken();
        if (userId == 0)
            return Unauthorized();

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var application = await _applicationService.CreateApplicationAsync(userId, dto);
            return CreatedAtAction(nameof(GetApplication), new { id = application.Id }, application);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Erstellen der Bewerbung");
            return BadRequest(new { message = "Bewerbung konnte nicht erstellt werden" });
        }
    }

    /// <summary>
    /// Aktualisiert eine Bewerbung
    /// </summary>
    /// <param name="id">Application ID</param>
    /// <param name="dto">Update Daten</param>
    /// <returns>Aktualisierte Bewerbung</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApplicationDto>> UpdateApplication(int id, [FromBody] UpdateApplicationDto dto)
    {
        var userId = GetUserIdFromToken();
        if (userId == 0)
            return Unauthorized();

        try
        {
            var application = await _applicationService.UpdateApplicationAsync(id, userId, dto);
            if (application == null)
                return NotFound();

            return Ok(application);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Aktualisieren der Bewerbung {ApplicationId}", id);
            return BadRequest(new { message = "Bewerbung konnte nicht aktualisiert werden" });
        }
    }

    /// <summary>
    /// Löscht eine Bewerbung
    /// </summary>
    /// <param name="id">Application ID</param>
    /// <returns>Success Message</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteApplication(int id)
    {
        var userId = GetUserIdFromToken();
        if (userId == 0)
            return Unauthorized();

        var success = await _applicationService.DeleteApplicationAsync(id, userId);
        if (!success)
            return NotFound();

        return NoContent();
    }

    // ==================== STATUS FILTER ====================

    /// <summary>
    /// Holt Bewerbungen nach Status gefiltert
    /// </summary>
    /// <param name="status">Status (submitted, interview, accepted, rejected, waiting)</param>
    /// <returns>Gefilterte Bewerbungen</returns>
    [HttpGet("status/{status}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<ApplicationDto>>> GetApplicationsByStatus(string status)
    {
        var userId = GetUserIdFromToken();
        if (userId == 0)
            return Unauthorized();

        var applications = await _applicationService.GetApplicationsByStatusAsync(userId, status);
        return Ok(applications);
    }

    // ==================== STATISTICS ====================

    /// <summary>
    /// Holt Statistiken für Bewerbungen
    /// </summary>
    /// <returns>Statistik-Daten</returns>
    [HttpGet("stats/overview")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApplicationStatsDto>> GetStatistics()
    {
        var userId = GetUserIdFromToken();
        if (userId == 0)
            return Unauthorized();

        var stats = await _applicationService.GetStatisticsAsync(userId);
        return Ok(stats);
    }

    // ==================== REMINDERS ====================

    /// <summary>
    /// Erstellt einen Reminder für eine Bewerbung
    /// </summary>
    /// <param name="applicationId">Application ID</param>
    /// <param name="dto">Reminder Daten</param>
    /// <returns>Erstellter Reminder</returns>
    [HttpPost("{applicationId}/reminders")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApplicationReminderDto>> CreateReminder(int applicationId, [FromBody] CreateReminderDto dto)
    {
        var userId = GetUserIdFromToken();
        if (userId == 0)
            return Unauthorized();

        try
        {
            var reminder = await _applicationService.CreateReminderAsync(applicationId, userId, dto);
            return CreatedAtAction(nameof(GetApplication), new { id = applicationId }, reminder);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Erstellen des Reminders");
            return BadRequest(new { message = "Reminder konnte nicht erstellt werden" });
        }
    }

    /// <summary>
    /// Markiert einen Reminder als abgeschlossen
    /// </summary>
    /// <param name="reminderId">Reminder ID</param>
    /// <returns>Success Message</returns>
    [HttpPut("reminders/{reminderId}/complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompleteReminder(int reminderId)
    {
        var userId = GetUserIdFromToken();
        if (userId == 0)
            return Unauthorized();

        var success = await _applicationService.CompleteReminderAsync(reminderId, userId);
        if (!success)
            return NotFound();

        return Ok(new { message = "Reminder markiert als abgeschlossen" });
    }

    /// <summary>
    /// Löscht einen Reminder
    /// </summary>
    /// <param name="reminderId">Reminder ID</param>
    /// <returns>Success Message</returns>
    [HttpDelete("reminders/{reminderId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteReminder(int reminderId)
    {
        var userId = GetUserIdFromToken();
        if (userId == 0)
            return Unauthorized();

        var success = await _applicationService.DeleteReminderAsync(reminderId, userId);
        if (!success)
            return NotFound();

        return NoContent();
    }

    // ==================== NOTES ====================

    /// <summary>
    /// Erstellt eine Notiz für eine Bewerbung
    /// </summary>
    /// <param name="applicationId">Application ID</param>
    /// <param name="dto">Notiz Daten</param>
    /// <returns>Erstellte Notiz</returns>
    [HttpPost("{applicationId}/notes")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApplicationNoteDto>> CreateNote(int applicationId, [FromBody] CreateNoteDto dto)
    {
        var userId = GetUserIdFromToken();
        if (userId == 0)
            return Unauthorized();

        try
        {
            var note = await _applicationService.CreateNoteAsync(applicationId, userId, dto);
            return CreatedAtAction(nameof(GetApplication), new { id = applicationId }, note);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Erstellen der Notiz");
            return BadRequest(new { message = "Notiz konnte nicht erstellt werden" });
        }
    }

    /// <summary>
    /// Löscht eine Notiz
    /// </summary>
    /// <param name="noteId">Note ID</param>
    /// <returns>Success Message</returns>
    [HttpDelete("notes/{noteId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteNote(int noteId)
    {
        var userId = GetUserIdFromToken();
        if (userId == 0)
            return Unauthorized();

        var success = await _applicationService.DeleteNoteAsync(noteId, userId);
        if (!success)
            return NotFound();

        return NoContent();
    }

    // ==================== HELPER ====================

    /// <summary>
    /// Extrahiert die User ID aus dem JWT Token
    /// </summary>
    private int GetUserIdFromToken()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
            return userId;

        return 0;
    }
}