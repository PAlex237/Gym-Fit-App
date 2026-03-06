namespace GymFit.API.Models.Requests
{
    public class UpdateTrainerRequest
    {
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        
    public string Role { get; set; } = null!;
    }
}
