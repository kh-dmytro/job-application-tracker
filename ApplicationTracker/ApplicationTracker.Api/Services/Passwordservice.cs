using System.Security.Cryptography;
using System.Text;

namespace ApplicationTracker.API.Services;

/// <summary>
/// Service für Password Hashing und Validierung
/// </summary>
public interface IPasswordService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}

public class PasswordService : IPasswordService
{
    /// <summary>
    /// Hasht ein Password mit SHA256
    /// </summary>
    /// <param name="password">Das zu hashende Password</param>
    /// <returns>Gehashtes Password (Hex-String)</returns>
    public string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Password darf nicht leer sein");

        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(hashedBytes);
        }
    }

    /// <summary>
    /// Validiert ein Password gegen einen Hash
    /// </summary>
    /// <param name="password">Das zu validierende Password</param>
    /// <param name="hash">Der gespeicherte Hash</param>
    /// <returns>True wenn Password korrekt, False sonst</returns>
    public bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash))
            return false;

        var hashOfInput = HashPassword(password);
        return hashOfInput.Equals(hash, StringComparison.OrdinalIgnoreCase);
    }
}