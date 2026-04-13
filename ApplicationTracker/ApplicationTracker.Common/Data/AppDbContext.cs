using ApplicationTracker.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace ApplicationTracker.Common.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

     public DbSet<User> Users { get; set; }

    /// <summary>
    /// Applications Tabelle - Alle Bewerbungen
    /// Verwendung: _context.Applications.Where(a => a.UserId == id)
    /// </summary>
    public DbSet<Application> Applications { get; set; }

    /// <summary>
    /// ApplicationReminders Tabelle - Alle Reminders
    /// Verwendung: _context.ApplicationReminders.Where(r => !r.IsCompleted)
    /// </summary>
    public DbSet<ApplicationReminder> ApplicationReminders { get; set; }

    /// <summary>
    /// ApplicationNotes Tabelle - Alle Notizen
    /// Verwendung: _context.ApplicationNotes.Where(n => n.ApplicationId == id)
    /// </summary>
    public DbSet<ApplicationNote> ApplicationNotes { get; set; }


    // ==================== 2. OnModelCreating ====================
    // Hier wird die gesamte Datenbankstruktur konfiguriert
    // Entity-Beziehungen, Indizes, Constraints, etc.
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ============ USER ENTITY KONFIGURATION ============
        
        // Primärschlüssel definieren
        modelBuilder.Entity<User>()
            .HasKey(u => u.Id);

        // Email muss eindeutig sein (UNIQUE Constraint)
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("IX_Users_Email_Unique");

        // Index für schnellere Filterung nach IsActive
        modelBuilder.Entity<User>()
            .HasIndex(u => u.IsActive)
            .HasDatabaseName("IX_Users_IsActive");

        // Index für Sortierung nach CreatedAt
        modelBuilder.Entity<User>()
            .HasIndex(u => u.CreatedAt)
            .HasDatabaseName("IX_Users_CreatedAt");

        // Beziehung: 1 User -> N Applications
        // Wenn User gelöscht wird, werden alle Applications auch gelöscht (Cascade)
        modelBuilder.Entity<User>()
            .HasMany(u => u.Applications)
            .WithOne(a => a.User)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_Applications_Users");

        // Default Wert für IsActive
        modelBuilder.Entity<User>()
            .Property(u => u.IsActive)
            .HasDefaultValue(true);

        // CreatedAt wird automatisch gesetzt
        modelBuilder.Entity<User>()
            .Property(u => u.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAdd();


        // ============ APPLICATION ENTITY KONFIGURATION ============

        // Primärschlüssel definieren
        modelBuilder.Entity<Application>()
            .HasKey(a => a.Id);

        // Composite Index: Häufige Query nach UserId und Status
        // Beispiel: "Alle eingereichten Bewerbungen von User XY"
        modelBuilder.Entity<Application>()
            .HasIndex(a => new { a.UserId, a.Status })
            .HasDatabaseName("IX_Applications_UserId_Status");

        // Index für Filterung nach Status
        modelBuilder.Entity<Application>()
            .HasIndex(a => a.Status)
            .HasDatabaseName("IX_Applications_Status");

        // Index für Sortierung nach Eingabedatum
        modelBuilder.Entity<Application>()
            .HasIndex(a => a.SubmittedDate)
            .HasDatabaseName("IX_Applications_SubmittedDate");

        // Index für Interview-Planung
        modelBuilder.Entity<Application>()
            .HasIndex(a => a.InterviewDate)
            .HasDatabaseName("IX_Applications_InterviewDate");

        // Index für Suche nach Unternehmensnamen
        modelBuilder.Entity<Application>()
            .HasIndex(a => a.Company)
            .HasDatabaseName("IX_Applications_Company");

        // Beziehung: 1 Application -> N Reminders
        // Wenn Application gelöscht wird, werden auch alle Reminders gelöscht
        modelBuilder.Entity<Application>()
            .HasMany(a => a.Reminders)
            .WithOne(r => r.Application)
            .HasForeignKey(r => r.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_ApplicationReminders_Applications");

        // Beziehung: 1 Application -> N Notes
        // Wenn Application gelöscht wird, werden auch alle Notes gelöscht
        modelBuilder.Entity<Application>()
            .HasMany(a => a.Notes_Collection)
            .WithOne(n => n.Application)
            .HasForeignKey(n => n.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_ApplicationNotes_Applications");

        // Default Status ist "submitted"
        modelBuilder.Entity<Application>()
            .Property(a => a.Status)
            .HasDefaultValue("submitted");

        // Maximale Länge für Notes
        modelBuilder.Entity<Application>()
            .Property(a => a.Notes)
            .HasMaxLength(2000);

        // CreatedAt wird automatisch gesetzt
        modelBuilder.Entity<Application>()
            .Property(a => a.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAdd();


        // ============ APPLICATION REMINDER KONFIGURATION ============

        // Primärschlüssel
        modelBuilder.Entity<ApplicationReminder>()
            .HasKey(r => r.Id);

        // Index für schnelle Filterung nach ApplicationId
        modelBuilder.Entity<ApplicationReminder>()
            .HasIndex(r => r.ApplicationId)
            .HasDatabaseName("IX_ApplicationReminders_ApplicationId");

        // Index für zeitbasierte Queries (z.B. "alle anstehenden Reminders")
        modelBuilder.Entity<ApplicationReminder>()
            .HasIndex(r => r.ReminderDate)
            .HasDatabaseName("IX_ApplicationReminders_ReminderDate");

        // Index für "zeige mir alle unvollständigen Reminders"
        modelBuilder.Entity<ApplicationReminder>()
            .HasIndex(r => r.IsCompleted)
            .HasDatabaseName("IX_ApplicationReminders_IsCompleted");

        // Default: Reminder ist nicht abgeschlossen
        modelBuilder.Entity<ApplicationReminder>()
            .Property(r => r.IsCompleted)
            .HasDefaultValue(false);

        // CreatedAt wird automatisch gesetzt
        modelBuilder.Entity<ApplicationReminder>()
            .Property(r => r.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAdd();


        // ============ APPLICATION NOTE KONFIGURATION ============

        // Primärschlüssel
        modelBuilder.Entity<ApplicationNote>()
            .HasKey(n => n.Id);

        // Index für schnelle Filterung nach ApplicationId
        modelBuilder.Entity<ApplicationNote>()
            .HasIndex(n => n.ApplicationId)
            .HasDatabaseName("IX_ApplicationNotes_ApplicationId");

        // Index für zeitbasierte Sortierung
        modelBuilder.Entity<ApplicationNote>()
            .HasIndex(n => n.CreatedAt)
            .HasDatabaseName("IX_ApplicationNotes_CreatedAt");

        // Maximale Länge für Content
        modelBuilder.Entity<ApplicationNote>()
            .Property(n => n.Content)
            .HasMaxLength(2000);

        // CreatedAt wird automatisch gesetzt
        modelBuilder.Entity<ApplicationNote>()
            .Property(n => n.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAdd();
    }
}