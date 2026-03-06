namespace GymFit.API.Models
{
    public class Course
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Optional: dacă vrei să știi ce trainer ține clasa
        public int? TrainerId { get; set; }
        public AppUser? Trainer { get; set; }

        // Relație cu Schedule
        public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    }
}
