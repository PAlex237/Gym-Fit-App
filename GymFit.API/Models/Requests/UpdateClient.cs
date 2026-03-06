namespace GymFit.API.Models.Requests
{public class UpdateClientRequest
{
    public string FullName { get; set; }= null!;
    public string Email { get; set; }= null!;
    public string Role { get; set; } = null!;// adaugă asta dacă nu există
}

}
