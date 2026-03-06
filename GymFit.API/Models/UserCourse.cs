
using GymFit.API.Models;

public class UserCourse
{
    public int UserId { get; set; }
    public required AppUser AppUser { get; set; }

    public int CourseId { get; set; }
    public required Course Course { get; set; }
}
