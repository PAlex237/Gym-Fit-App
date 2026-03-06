namespace GymFit.API.Models
{
    public class Schedule
    {
        public int Id { get; set; }

        public int ClientId { get; set; }
        public AppUser Client { get; set; } = null!;

        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;

        public DateTime ScheduledAt { get; set; } = DateTime.UtcNow;
    }
}
