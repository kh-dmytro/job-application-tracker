using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ApplicationTracker.Common.Models;

[Table("Applications")]
public class Application
{
    /// <summary>
    /// Primärschlüssel
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    // ==================== Basis-Informationen ====================

    /// <summary>
    /// Name des Unternehmens
    /// </summary>
    [Required(ErrorMessage = "Unternehmen ist erforderlich")]
    [StringLength(255, ErrorMessage = "Unternehmen darf nicht länger als 255 Zeichen sein")]
   
    public string Company { get; set; } = string.Empty;

    /// <summary>
    /// Position/Jobtitel
    /// </summary>
    [Required(ErrorMessage = "Position ist erforderlich")]
    [StringLength(255, ErrorMessage = "Position darf nicht länger als 255 Zeichen sein")]
    public string Position { get; set; } = string.Empty;

    /// <summary>
    /// Status der Bewerbung: submitted, interview, accepted, rejected, waiting
    /// </summary>
    [Required(ErrorMessage = "Status ist erforderlich")]
    [StringLength(50, ErrorMessage = "Status darf nicht länger als 50 Zeichen sein")]
    public string Status { get; set; } = "submitted";

    /// <summary>
    /// Datum der Bewerbungseinreichung
    /// </summary>
    [Required(ErrorMessage = "Eingabedatum ist erforderlich")]
    public DateTime SubmittedDate { get; set; } = DateTime.UtcNow;

    // ==================== Termin-Informationen ====================

    /// <summary>
    /// Datum und Zeit des Interviews (falls vereinbart)
    /// </summary>
    public DateTime? InterviewDate { get; set; }

    /// <summary>
    /// Datum der Antwort vom Unternehmen
    /// </summary>
    public DateTime? ResponseDate { get; set; }

    // ==================== Unternehmen-Details ====================

    /// <summary>
    /// Link zur Stellenanzeige
    /// </summary>
    [StringLength(500, ErrorMessage = "URL darf nicht länger als 500 Zeichen sein")]
    [Url(ErrorMessage = "Ungültiges URL-Format")]
    public string? JobUrl { get; set; }

    /// <summary>
    /// Website des Unternehmens
    /// </summary>
    [StringLength(500, ErrorMessage = "Website darf nicht länger als 500 Zeichen sein")]
    [Url(ErrorMessage = "Ungültiges URL-Format")]
    public string? CompanyWebsite { get; set; }

    // ==================== Kontakt-Informationen ====================

    /// <summary>
    /// Name des Kontakts (z.B. Recruiter, Hiring Manager)
    /// </summary>
    [StringLength(255, ErrorMessage = "Kontakt darf nicht länger als 255 Zeichen sein")]
    public string? Contact { get; set; }

    /// <summary>
    /// E-Mail des Kontakts
    /// </summary>
    [StringLength(255, ErrorMessage = "E-Mail darf nicht länger als 255 Zeichen sein")]
    [EmailAddress(ErrorMessage = "Ungültiges E-Mail-Format")]
    public string? ContactEmail { get; set; }

    /// <summary>
    /// Telefonnummer des Kontakts
    /// </summary>
    [StringLength(255, ErrorMessage = "Telefonnummer darf nicht länger als 255 Zeichen sein")]
    [Phone(ErrorMessage = "Ungültiges Telefonformat")]
    public string? ContactPhone { get; set; }

    // ==================== Verhandlungs-Details ====================

    /// <summary>
    /// Gehalt oder Gehaltsrange
    /// </summary>
    [StringLength(500, ErrorMessage = "Gehalt darf nicht länger als 500 Zeichen sein")]
    public string? Salary { get; set; }

    /// <summary>
    /// Freitextnotizen zur Bewerbung
    /// </summary>
    [StringLength(2000, ErrorMessage = "Notizen darf nicht länger als 2000 Zeichen sein")]
    public string? Notes { get; set; }

    // ==================== Profile Links ====================

    /// <summary>
    /// Link zum LinkedIn Profil oder zur Unternehmensseite
    /// </summary>
    [StringLength(500, ErrorMessage = "LinkedIn URL darf nicht länger als 500 Zeichen sein")]
    [Url(ErrorMessage = "Ungültiges URL-Format")]
    public string? LinkedinProfile { get; set; }

    /// <summary>
    /// Link zum XING Profil oder zur Unternehmensseite
    /// </summary>
    [StringLength(500, ErrorMessage = "XING URL darf nicht länger als 500 Zeichen sein")]
    [Url(ErrorMessage = "Ungültiges URL-Format")]
    public string? XingProfile { get; set; }

    /// <summary>
    /// Wurde LinkedIn Profil verlinkt?
    /// </summary>
    public bool LinkedinProfileLinked { get; set; } = false;

    /// <summary>
    /// Wurde XING Profil verlinkt?
    /// </summary>
    public bool XingProfileLinked { get; set; } = false;

    // ==================== Externe Daten ====================

    /// <summary>
    /// JSON-Daten vom LinkedIn Profil (als JSON serialized)
    /// </summary>
    [StringLength(500, ErrorMessage = "LinkedIn Daten darf nicht länger als 500 Zeichen sein")]
    public string? LinkedinData { get; set; }

    /// <summary>
    /// JSON-Daten vom XING Profil (als JSON serialized)
    /// </summary>
    [StringLength(500, ErrorMessage = "XING Daten darf nicht länger als 500 Zeichen sein")]
    public string? XingData { get; set; }

    // ==================== Fremdschlüssel ====================

    /// <summary>
    /// User ID - Fremdschlüssel zu User
    /// </summary>
    [Required(ErrorMessage = "User ID ist erforderlich")]
    public int UserId { get; set; }

    // ==================== Audit Trail ====================

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
    /// Referenz zum User (Navigation Property)
    /// </summary>
    [ForeignKey(nameof(UserId))]
    [InverseProperty(nameof(User.Applications))]
    public virtual User? User { get; set; }

    /// <summary>
    /// Alle Reminder für diese Bewerbung
    /// </summary>
    [InverseProperty(nameof(ApplicationReminder.Application))]
    public virtual ICollection<ApplicationReminder> Reminders { get; set; } = [];

    /// <summary>
    /// Alle Notizen für diese Bewerbung
    /// </summary>
    [InverseProperty(nameof(ApplicationNote.Application))]
    public virtual ICollection<ApplicationNote> Notes_Collection { get; set; } = [];
}
