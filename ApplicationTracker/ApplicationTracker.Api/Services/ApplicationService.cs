using ApplicationTracker.Api.Models;
using ApplicationTracker.Common.Data;
using Microsoft.EntityFrameworkCore;

namespace ApplicationTracker.API.Services;

/// <summary>
/// Service für Application CRUD und Business Logic
/// </summary>
public interface IApplicationService
{
    Task<IEnumerable<Application>> GetApplicationsAsync(int userId);
    Task<Application?> GetApplicationAsync(int applicationId, int userId);
    Task<Application> CreateApplicationAsync(int userId, CreateApplicationModel model);
    Task<Application?> UpdateApplicationAsync(int applicationId, int userId, UpdateApplication );
    Task<bool> DeleteApplicationAsync(int applicationId, int userId);
    Task<ApplicationStats> GetStatisticsAsync(int userId);
    Task<IEnumerable<Application>> GetApplicationsByStatusAsync(int userId, string status);
    
    // Reminders
    Task<ApplicationReminder> CreateReminderAsync(int applicationId, int userId, CreateReminder );
    Task<bool> DeleteReminderAsync(int reminderId, int userId);
    Task<bool> CompleteReminderAsync(int reminderId, int userId);
    
    // Notes
    Task<ApplicationNote> CreateNoteAsync(int applicationId, int userId, CreateNote );
    Task<bool> DeleteNoteAsync(int noteId, int userId);
}

public class ApplicationService : IApplicationService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ApplicationService> _logger;

    public ApplicationService(AppDbContext context, ILogger<ApplicationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // ==================== APPLICATIONS CRUD ====================

    /// <summary>
    /// Holt alle Bewerbungen eines Users
    /// </summary>
    public async Task<IEnumerable<Application>> GetApplicationsAsync(int userId)
    {
        try
        {
            var applications = await _context.Applications
                .Where(a => a.UserId == userId)
                .Include(a => a.Reminders.Where(r => !r.IsCompleted))
                .Include(a => a.Notes_Collection)
                .OrderByDescending(a => a.SubmittedDate)
                .ToListAsync();

            return applications.Select(MapTo).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen der Bewerbungen für User {UserId}", userId);
            return [];
        }
    }

    /// <summary>
    /// Holt eine einzelne Bewerbung
    /// </summary>
    public async Task<Application?> GetApplicationAsync(int applicationId, int userId)
    {
        try
        {
            var application = await _context.Applications
                .Where(a => a.Id == applicationId && a.UserId == userId)
                .Include(a => a.Reminders)
                .Include(a => a.Notes_Collection)
                .FirstOrDefaultAsync();

            return application != null ? MapTo(application) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen der Bewerbung {ApplicationId}", applicationId);
            return null;
        }
    }

    /// <summary>
    /// Erstellt eine neue Bewerbung
    /// </summary>
    public async Task<Application> CreateApplicationAsync(int userId, CreateApplicationModel model)
    {
        try
        {
            var application = new Application
            {
                UserId = userId,
                Company = model.Company,
                Position = model.Position,
                Status = model.Status ?? "submitted",
                SubmittedDate = model.SubmittedDate,
                JobUrl = model.JobUrl,
                CompanyWebsite = model.CompanyWebsite,
                Contact = model.Contact,
                ContactEmail = model.ContactEmail,
                ContactPhone = model.ContactPhone,
                Salary = model.Salary,
                Notes = model.Notes,
                LinkedinProfile = model.LinkedinProfile,
                XingProfile = model.XingProfile,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Applications.Add(application);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Bewerbung erstellt: {Company} - {Position} für User {UserId}", 
                application.Company, application.Position, userId);

            return MapTo(application);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Erstellen der Bewerbung für User {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Aktualisiert eine Bewerbung
    /// </summary>
    public async Task<Application?> UpdateApplicationAsync(int applicationId, int userId, UpdateApplication )
    {
        try
        {
            var application = await _context.Applications
                .Include(a => a.Reminders)
                .Include(a => a.Notes_Collection)
                .FirstOrDefaultAsync(a => a.Id == applicationId && a.UserId == userId);

            if (application == null)
                return null;

            // Update nur nicht-leere Felder
            if (!string.IsNullOrEmpty(.Company))
                application.Company = .Company;
            if (!string.IsNullOrEmpty(.Position))
                application.Position = .Position;
            if (!string.IsNullOrEmpty(.Status))
                application.Status = .Status;
            if (.InterviewDate.HasValue)
                application.InterviewDate = .InterviewDate;
            if (.ResponseDate.HasValue)
                application.ResponseDate = .ResponseDate;
            if (!string.IsNullOrEmpty(.JobUrl))
                application.JobUrl = .JobUrl;
            if (!string.IsNullOrEmpty(.CompanyWebsite))
                application.CompanyWebsite = .CompanyWebsite;
            if (!string.IsNullOrEmpty(.Contact))
                application.Contact = .Contact;
            if (!string.IsNullOrEmpty(.ContactEmail))
                application.ContactEmail = .ContactEmail;
            if (!string.IsNullOrEmpty(.ContactPhone))
                application.ContactPhone = .ContactPhone;
            if (!string.IsNullOrEmpty(.Salary))
                application.Salary = .Salary;
            if (!string.IsNullOrEmpty(.Notes))
                application.Notes = .Notes;
            if (!string.IsNullOrEmpty(.LinkedinProfile))
                application.LinkedinProfile = .LinkedinProfile;
            if (!string.IsNullOrEmpty(.XingProfile))
                application.XingProfile = .XingProfile;

            application.UpdatedAt = DateTime.UtcNow;

            _context.Applications.Update(application);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Bewerbung {ApplicationId} aktualisiert", applicationId);

            return MapTo(application);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Aktualisieren der Bewerbung {ApplicationId}", applicationId);
            throw;
        }
    }

    /// <summary>
    /// Löscht eine Bewerbung
    /// </summary>
    public async Task<bool> DeleteApplicationAsync(int applicationId, int userId)
    {
        try
        {
            var application = await _context.Applications
                .FirstOrDefaultAsync(a => a.Id == applicationId && a.UserId == userId);

            if (application == null)
                return false;

            _context.Applications.Remove(application);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Bewerbung {ApplicationId} gelöscht", applicationId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Löschen der Bewerbung {ApplicationId}", applicationId);
            throw;
        }
    }

    // ==================== STATISTICS ====================

    /// <summary>
    /// Holt Statistiken für Bewerbungen
    /// </summary>
    public async Task<ApplicationStats> GetStatisticsAsync(int userId)
    {
        try
        {
            var applications = await _context.Applications
                .Where(a => a.UserId == userId)
                .ToListAsync();

            var total = applications.Count;
            var submitted = applications.Count(a => a.Status == "submitted");
            var interview = applications.Count(a => a.Status == "interview");
            var accepted = applications.Count(a => a.Status == "accepted");
            var rejected = applications.Count(a => a.Status == "rejected");
            var waiting = applications.Count(a => a.Status == "waiting");

            var successRate = total > 0 ? (double)accepted / total * 100 : 0;
            var upcomingInterviews = applications.Count(a => 
                a.Status == "interview" && 
                a.InterviewDate.HasValue && 
                a.InterviewDate > DateTime.UtcNow);

            return new ApplicationStats
            {
                Total = total,
                Submitted = submitted,
                Interview = interview,
                Accepted = accepted,
                Rejected = rejected,
                Waiting = waiting,
                SuccessRate = Math.Round(successRate, 2),
                UpcomingInterviews = upcomingInterviews
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen von Statistiken für User {UserId}", userId);
            return new ApplicationStats();
        }
    }

    /// <summary>
    /// Holt Bewerbungen nach Status gefiltert
    /// </summary>
    public async Task<IEnumerable<Application>> GetApplicationsByStatusAsync(int userId, string status)
    {
        try
        {
            var applications = await _context.Applications
                .Where(a => a.UserId == userId && a.Status == status)
                .Include(a => a.Reminders.Where(r => !r.IsCompleted))
                .Include(a => a.Notes_Collection)
                .OrderByDescending(a => a.SubmittedDate)
                .ToListAsync();

            return applications.Select(MapTo).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen der Bewerbungen mit Status {Status} für User {UserId}", status, userId);
            return [];
        }
    }

    // ==================== REMINDERS ====================

    /// <summary>
    /// Erstellt einen Reminder
    /// </summary>
    public async Task<ApplicationReminder> CreateReminderAsync(int applicationId, int userId, CreateReminder )
    {
        try
        {
            var application = await _context.Applications
                .FirstOrDefaultAsync(a => a.Id == applicationId && a.UserId == userId);

            if (application == null)
                throw new InvalidOperationException("Bewerbung nicht gefunden");

            var reminder = new ApplicationReminder
            {
                ApplicationId = applicationId,
                Title = .Title,
                Description = .Description,
                ReminderDate = .ReminderDate,
                CreatedAt = DateTime.UtcNow
            };

            _context.ApplicationReminders.Add(reminder);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Reminder erstellt für Bewerbung {ApplicationId}", applicationId);

            return MapReminderTo(reminder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Erstellen des Reminders für Bewerbung {ApplicationId}", applicationId);
            throw;
        }
    }

    /// <summary>
    /// Markiert einen Reminder als abgeschlossen
    /// </summary>
    public async Task<bool> CompleteReminderAsync(int reminderId, int userId)
    {
        try
        {
            var reminder = await _context.ApplicationReminders
                .Include(r => r.Application)
                .FirstOrDefaultAsync(r => r.Id == reminderId && r.Application!.UserId == userId);

            if (reminder == null)
                return false;

            reminder.IsCompleted = true;
            reminder.CompletedDate = DateTime.UtcNow;

            _context.ApplicationReminders.Update(reminder);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Reminder {ReminderId} als abgeschlossen markiert", reminderId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Abschließen des Reminders {ReminderId}", reminderId);
            throw;
        }
    }

    /// <summary>
    /// Löscht einen Reminder
    /// </summary>
    public async Task<bool> DeleteReminderAsync(int reminderId, int userId)
    {
        try
        {
            var reminder = await _context.ApplicationReminders
                .Include(r => r.Application)
                .FirstOrDefaultAsync(r => r.Id == reminderId && r.Application!.UserId == userId);

            if (reminder == null)
                return false;

            _context.ApplicationReminders.Remove(reminder);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Reminder {ReminderId} gelöscht", reminderId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Löschen des Reminders {ReminderId}", reminderId);
            throw;
        }
    }

    // ==================== NOTES ====================

    /// <summary>
    /// Erstellt eine Notiz
    /// </summary>
    public async Task<ApplicationNote> CreateNoteAsync(int applicationId, int userId, CreateNote )
    {
        try
        {
            var application = await _context.Applications
                .FirstOrDefaultAsync(a => a.Id == applicationId && a.UserId == userId);

            if (application == null)
                throw new InvalidOperationException("Bewerbung nicht gefunden");

            var note = new ApplicationNote
            {
                ApplicationId = applicationId,
                Content = .Content,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.ApplicationNotes.Add(note);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Notiz erstellt für Bewerbung {ApplicationId}", applicationId);

            return MapNoteTo(note);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Erstellen der Notiz für Bewerbung {ApplicationId}", applicationId);
            throw;
        }
    }

    /// <summary>
    /// Löscht eine Notiz
    /// </summary>
    public async Task<bool> DeleteNoteAsync(int noteId, int userId)
    {
        try
        {
            var note = await _context.ApplicationNotes
                .Include(n => n.Application)
                .FirstOrDefaultAsync(n => n.Id == noteId && n.Application!.UserId == userId);

            if (note == null)
                return false;

            _context.ApplicationNotes.Remove(note);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Notiz {NoteId} gelöscht", noteId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Löschen der Notiz {NoteId}", noteId);
            throw;
        }
    }

    // ==================== HELPER METHODS ====================

    private Application MapTo(Application application)
    {
        return new Application
        {
            Id = application.Id,
            Company = application.Company,
            Position = application.Position,
            Status = application.Status,
            SubmittedDate = application.SubmittedDate,
            InterviewDate = application.InterviewDate,
            ResponseDate = application.ResponseDate,
            JobUrl = application.JobUrl,
            CompanyWebsite = application.CompanyWebsite,
            Contact = application.Contact,
            ContactEmail = application.ContactEmail,
            ContactPhone = application.ContactPhone,
            Salary = application.Salary,
            Notes = application.Notes,
            LinkedinProfile = application.LinkedinProfile,
            XingProfile = application.XingProfile,
            LinkedinProfileLinked = application.LinkedinProfileLinked,
            XingProfileLinked = application.XingProfileLinked,
            CreatedAt = application.CreatedAt,
            UpdatedAt = application.UpdatedAt,
            Reminders = application.Reminders.Select(MapReminderTo).ToList(),
            Notes_Collection = application.Notes_Collection.Select(MapNoteTo).ToList()
        };
    }

    private ApplicationReminder MapReminderTo(ApplicationReminder reminder)
    {
        return new ApplicationReminder
        {
            Id = reminder.Id,
            Title = reminder.Title,
            Description = reminder.Description,
            ReminderDate = reminder.ReminderDate,
            IsCompleted = reminder.IsCompleted,
            CompletedDate = reminder.CompletedDate,
            CreatedAt = reminder.CreatedAt
        };
    }

    private ApplicationNote MapNoteTo(ApplicationNote note)
    {
        return new ApplicationNote
        {
            Id = note.Id,
            Content = note.Content,
            CreatedAt = note.CreatedAt,
            UpdatedAt = note.UpdatedAt
        };
    }
}