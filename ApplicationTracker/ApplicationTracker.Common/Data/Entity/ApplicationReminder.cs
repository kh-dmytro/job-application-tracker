// <summary>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// ApplicationReminder Entity - Speichert Erinnerungen für Bewerbungen
/// </summary>
[Table("ApplicationReminders")]
public class ApplicationReminder
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
    /// Titel/Name des Reminders
    /// Beispiele: "Follow-up Email", "Interview vorbereiten", "Anrufen"
    /// </summary>
    [Required(ErrorMessage = "Titel ist erforderlich")]
    [StringLength(500, ErrorMessage = "Titel darf nicht länger als 500 Zeichen sein")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Optionale Beschreibung des Reminders
    /// </summary>
    [StringLength(2000, ErrorMessage = "Beschreibung darf nicht länger als 2000 Zeichen sein")]
    public string? Description { get; set; }

    /// <summary>
    /// Zeitpunkt, wann der Reminder ausgelöst werden soll
    /// </summary>
    [Required(ErrorMessage = "Datum ist erforderlich")]
    public DateTime ReminderDate { get; set; }

    /// <summary>
    /// Wurde der Reminder abgeschlossen/erledigt?
    /// </summary>
    public bool IsCompleted { get; set; } = false;

    /// <summary>
    /// Zeitpunkt, wann der Reminder abgeschlossen wurde
    /// </summary>
    public DateTime? CompletedDate { get; set; }

    /// <summary>
    /// Zeitstempel der Erstellung
    /// </summary>
    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ==================== Navigation Properties ====================

    /// <summary>
    /// Referenz zur Application (Navigation Property)
    /// </summary>
    [ForeignKey(nameof(ApplicationId))]
    [InverseProperty(nameof(Application.Reminders))]
    public virtual Application? Application { get; set; }
}
