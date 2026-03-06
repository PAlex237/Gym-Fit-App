using GymFit.API.Models;

public class AppUser
{
    public int Id { get; set; }
    public required string FullName { get; set; } = string.Empty;
    public required string Email { get; set; } = string.Empty;
    public required string PasswordHash { get; set; } = string.Empty;

    public string Role { get; set; } = "Client"; // "Admin" sau "Trainer"

    // Relație 1 Client - Multe Schedule (în loc de direct Course)
    public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
     public ICollection<Course> Courses { get; set; } = new List<Course>(); // dacă e Trainer
}
