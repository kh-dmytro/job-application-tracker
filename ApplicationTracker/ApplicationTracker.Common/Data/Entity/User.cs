using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApplicationTracker.Common.Models;

    [Table("Users")]
public class User
{
    /// <summary>
    /// Primärschlüssel
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// Eindeutige E-Mail-Adresse des Benutzers
    /// </summary>
    [Required(ErrorMessage = "E-Mail ist erforderlich")]
    [StringLength(255, ErrorMessage = "E-Mail darf nicht länger als 255 Zeichen sein")]
    [EmailAddress(ErrorMessage = "Ungültiges E-Mail-Format")]
    // [Index(IsUnique = true)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Vollständiger Name des Benutzers
    /// </summary>
    [StringLength(255, ErrorMessage = "Name darf nicht länger als 255 Zeichen sein")]
    public string? FullName { get; set; }

    /// <summary>
    /// Hash des Passworts (SHA256)
    /// </summary>
    [StringLength(500, ErrorMessage = "Passwort-Hash darf nicht länger als 500 Zeichen sein")]
    public string? PasswordHash { get; set; }

    // ==================== LinkedIn OAuth ====================

    /// <summary>
    /// Ist LinkedIn verbunden?
    /// </summary>
    public bool LinkedinConnected { get; set; } = false;

    /// <summary>
    /// LinkedIn Access Token für API Calls
    /// </summary>
    [StringLength(500, ErrorMessage = "Token darf nicht länger als 500 Zeichen sein")]
    public string? LinkedinAccessToken { get; set; }

    /// <summary>
    /// LinkedIn Refresh Token für Token-Erneuerung
    /// </summary>
    [StringLength(500, ErrorMessage = "Refresh Token darf nicht länger als 500 Zeichen sein")]
    public string? LinkedinRefreshToken { get; set; }

    /// <summary>
    /// Ablaufdatum des LinkedIn Access Tokens
    /// </summary>
    public DateTime? LinkedinTokenExpiry { get; set; }

    /// <summary>
    /// Eindeutige LinkedIn User ID
    /// </summary>
    [StringLength(500, ErrorMessage = "LinkedIn ID darf nicht länger als 500 Zeichen sein")]
    public string? LinkedinId { get; set; }

    // ==================== XING OAuth ====================

    /// <summary>
    /// Ist XING verbunden?
    /// </summary>
    public bool XingConnected { get; set; } = false;

    /// <summary>
    /// XING Access Token für API Calls
    /// </summary>
    [StringLength(500, ErrorMessage = "Token darf nicht länger als 500 Zeichen sein")]
    public string? XingAccessToken { get; set; }

    /// <summary>
    /// XING Refresh Token für Token-Erneuerung
    /// </summary>
    [StringLength(500, ErrorMessage = "Refresh Token darf nicht länger als 500 Zeichen sein")]
    public string? XingRefreshToken { get; set; }

    /// <summary>
    /// Ablaufdatum des XING Access Tokens
    /// </summary>
    public DateTime? XingTokenExpiry { get; set; }

    /// <summary>
    /// Eindeutige XING User ID
    /// </summary>
    [StringLength(500, ErrorMessage = "XING ID darf nicht länger als 500 Zeichen sein")]
    public string? XingId { get; set; }

    // ==================== Account Management ====================

    /// <summary>
    /// Zeitstempel der Kontoerstellung
    /// </summary>
    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Zeitstempel der letzten Aktualisierung
    /// </summary>
    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Ist das Benutzerkonto aktiv?
    /// </summary>
    public bool IsActive { get; set; } = true;

    // ==================== Navigation Properties ====================

    /// <summary>
    /// Alle Bewerbungen dieses Benutzers
    /// </summary>
    [InverseProperty(nameof(Application.User))]
    public virtual ICollection<Application> Applications { get; set; } = [];
}
