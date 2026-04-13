


/// <summary>
/// User Entity - Speichert Benutzerdaten und OAuth Tokens
/// </summary>


/// <summary>
/// Application Entity - Speichert Bewerbungsinformationen
/// </summary>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
/// <summary>
/// ApplicationNote Entity - Speichert Notizen zu Bewerbungen
/// </summary>
[Table("ApplicationNotes")]
public class ApplicationNote
{
    /// <summary>
    /// Primärschlüssel
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// Application ID - Fremdschlüssel zu Application
    /// </summary>
    [Required(ErrorMessage = "Application ID ist erforderlich")]
    public int ApplicationId { get; set; }

    /// <summary>
    /// Inhalt der Notiz
    /// Beispiele: "Interviewer war freundlich", "Technische Fragen zu Cloud", etc.
    /// </summary>
    [Required(ErrorMessage = "Inhalt ist erforderlich")]
    [StringLength(2000, ErrorMessage = "Notiz darf nicht länger als 2000 Zeichen sein")]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Zeitstempel der Erstellung
    /// </summary>
    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Zeitstempel der letzten Aktualisierung
    /// </summary>
    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // ==================== Navigation Properties ====================

    /// <summary>
    /// Referenz zur Application (Navigation Property)
    /// </summary>
    [ForeignKey(nameof(ApplicationId))]
    [InverseProperty(nameof(Application.Notes_Collection))]
    public virtual Application? Application { get; set; }
}