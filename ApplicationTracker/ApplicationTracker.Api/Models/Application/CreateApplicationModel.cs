namespace ApplicationTracker.Api.Models;

public class CreateApplicationModel
{
    public int UserId { get; set; } 
    public Company = model.Company,
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
}