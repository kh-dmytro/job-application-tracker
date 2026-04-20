using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace ApplicationTracker.API.Services;

/// <summary>
/// Service für JWT Token Generierung und Validierung
/// </summary>
public interface IJwtTokenService
{
    string GenerateToken(int userId, string email, string fullName);
    ClaimsPrincipal GetPrincipalFromToken(string token);
}

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtTokenService> _logger;

    public JwtTokenService(IConfiguration configuration, ILogger<JwtTokenService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Generiert einen JWT Token für einen User
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="email">User Email</param>
    /// <param name="fullName">User Vollständiger Name</param>
    /// <returns>JWT Token String</returns>
    public string GenerateToken(int userId, string email, string fullName)
    {
        try
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];
            var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "60");

            // Validierung
            if (string.IsNullOrEmpty(secretKey) || secretKey.Length < 32)
            {
                _logger.LogError("JWT Secret Key ist zu kurz oder nicht konfiguriert!");
                throw new InvalidOperationException("JWT Secret Key ist nicht korrekt konfiguriert");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Claims definieren
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Name, fullName ?? email),
                new Claim("sub", userId.ToString()),
                new Claim("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
            };

            // Token erstellen
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: credentials
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwt = tokenHandler.WriteToken(token);

            _logger.LogInformation("Token für User {UserId} ({Email}) generiert", userId, email);
            return jwt;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Generieren des JWT Tokens");
            throw;
        }
    }

    /// <summary>
    /// Validiert einen JWT Token und gibt die Claims zurück
    /// </summary>
    /// <param name="token">JWT Token String</param>
    /// <returns>ClaimsPrincipal mit User-Infos</returns>
    public ClaimsPrincipal GetPrincipalFromToken(string token)
    {
        try
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            var tokenHandler = new JwtSecurityTokenHandler();

            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return principal;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Validieren des Tokens");
            return null;
        }
    }
}