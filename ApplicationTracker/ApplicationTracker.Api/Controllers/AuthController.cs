using ApplicationTracker.Common;
using ApplicationTracker.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ApplicationTracker.API.Controllers;

/// <summary>
/// Controller für Authentication (Register, Login, Profile)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Registriert einen neuen User
    /// </summary>
    /// <param name="">Registrierungsdaten</param>
    /// <returns>AuthResponse mit Token und User</returns>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterUser )
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var (success, message, user, token) = await _authService.RegisterAsync();

        if (!success)
            return BadRequest(new { message });

        var response = new AuthResponse
        {
            Token = token,
            User = user,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        return Ok(response);
    }

    /// <summary>
    /// Meldet einen User an
    /// </summary>
    /// <param name="">Login Daten</param>
    /// <returns>AuthResponse mit Token und User</returns>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginUser )
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var (success, message, user, token) = await _authService.LoginAsync();

        if (!success)
            return Unauthorized(new { message });

        var response = new AuthResponse
        {
            Token = token,
            User = user,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        return Ok(response);
    }

    /// <summary>
    /// Holt die Daten des aktuellen Users
    /// </summary>
    /// <returns>User Daten</returns>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<User>> GetMe()
    {
        var userId = GetUserIdFromToken();
        if (userId == 0)
            return Unauthorized();

        var user = await _authService.GetUserByIdAsync(userId);
        if (user == null)
            return NotFound();

        return Ok(user);
    }

    /// <summary>
    /// Aktualisiert das User-Profil
    /// </summary>
    /// <param name="">Update Daten</param>
    /// <returns>Success Message</returns>
    [HttpPut("profile")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfile )
    {
        var userId = GetUserIdFromToken();
        if (userId == 0)
            return Unauthorized();

        var success = await _authService.UpdateProfileAsync(userId, );
        if (!success)
            return BadRequest(new { message = "Profil konnte nicht aktualisiert werden" });

        return Ok(new { message = "Profil erfolgreich aktualisiert" });
    }

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