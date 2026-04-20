using ApplicationTracker.Common.Data;
using ApplicationTracker.Common;
using ApplicationTracker.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace ApplicationTracker.API.Services;

/// <summary>
/// Service für Authentication (Register, Login)
/// </summary>
public interface IAuthService
{
    Task<(bool success, string message, User? user, string? token)> RegisterAsync(RegisterUser dto);
    Task<(bool success, string message, User? user, string? token)> LoginAsync(LoginUser dto);
    Task<User?> GetUserByIdAsync(int userId);
    Task<bool> UpdateProfileAsync(int userId, UpdateProfile dto);
}

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IPasswordService _passwordService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        AppDbContext context,
        IPasswordService passwordService,
        IJwtTokenService jwtTokenService,
        ILogger<AuthService> logger)
    {
        _context = context;
        _passwordService = passwordService;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    /// <summary>
    /// Registriert einen neuen User
    /// </summary>
    public async Task<(bool success, string message, User? user, string? token)> RegisterAsync(RegisterUser dto)
    {
        try
        {
            // Validierung
            if (string.IsNullOrWhiteSpace(dto.Email))
                return (false, "Email ist erforderlich", null, null);

            if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 6)
                return (false, "Passwort muss mindestens 6 Zeichen lang sein", null, null);

            // Prüfe ob Email bereits existiert
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (existingUser != null)
                return (false, "Diese Email-Adresse wird bereits verwendet", null, null);

            // Neuen User erstellen
            var user = new User
            {
                Email = dto.Email,
                FullName = dto.FullName ?? dto.Email,
                PasswordHash = _passwordService.HashPassword(dto.Password),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Token generieren
            var token = _jwtTokenService.GenerateToken(user.Id, user.Email, user.FullName);
            var user = MapToUser(user);

            _logger.LogInformation("User {Email} erfolgreich registriert", user.Email);
            return (true, "Registrierung erfolgreich", user, token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler bei der Registrierung");
            return (false, "Ein Fehler ist aufgetreten", null, null);
        }
    }

    /// <summary>
    /// Meldet einen User an
    /// </summary>
    public async Task<(bool success, string message, User? user, string? token)> LoginAsync(LoginUser dto)
    {
        try
        {
            // Validierung
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                return (false, "Email und Passwort sind erforderlich", null, null);

            // User suchen
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
            {
                _logger.LogWarning("Login versucht mit nicht existierender Email: {Email}", dto.Email);
                return (false, "Email oder Passwort ist falsch", null, null);
            }

            // Password validieren
            if (!_passwordService.VerifyPassword(dto.Password, user.PasswordHash))
            {
                _logger.LogWarning("Login versucht mit falschem Passwort für: {Email}", dto.Email);
                return (false, "Email oder Passwort ist falsch", null, null);
            }

            // Prüfe ob User aktiv ist
            if (!user.IsActive)
                return (false, "Dieses Benutzerkonto wurde deaktiviert", null, null);

            // Token generieren
            var token = _jwtTokenService.GenerateToken(user.Id, user.Email, user.FullName);
            var user = MapToUser(user);

            _logger.LogInformation("User {Email} erfolgreich angemeldet", user.Email);
            return (true, "Anmeldung erfolgreich", user, token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Login");
            return (false, "Ein Fehler ist aufgetreten", null, null);
        }
    }

    /// <summary>
    /// Holt User-Informationen
    /// </summary>
    public async Task<User?> GetUserByIdAsync(int userId)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            return user != null ? MapToUser(user) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen des Users {UserId}", userId);
            return null;
        }
    }

    /// <summary>
    /// Aktualisiert das User-Profil
    /// </summary>
    public async Task<bool> UpdateProfileAsync(int userId, UpdateProfile dto)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            // Vollständigen Namen aktualisieren
            if (!string.IsNullOrWhiteSpace(dto.FullName))
                user.FullName = dto.FullName;

            // Passwort aktualisieren wenn altes Passwort korrekt ist
            if (!string.IsNullOrWhiteSpace(dto.CurrentPassword) && !string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                if (!_passwordService.VerifyPassword(dto.CurrentPassword, user.PasswordHash))
                    return false;

                if (dto.NewPassword.Length < 6)
                    return false;

                user.PasswordHash = _passwordService.HashPassword(dto.NewPassword);
            }

            user.UpdatedAt = DateTime.UtcNow;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Profil für User {UserId} aktualisiert", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Aktualisieren des Profils für User {UserId}", userId);
            return false;
        }
    }

    private User MapToUser(User user)
    {
        return new User
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            LinkedinConnected = user.LinkedinConnected,
            XingConnected = user.XingConnected,
            LinkedinId = user.LinkedinId,
            XingId = user.XingId,
            CreatedAt = user.CreatedAt
        };
    }
}