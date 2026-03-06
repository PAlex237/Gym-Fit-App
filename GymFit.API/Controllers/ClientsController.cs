using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GymFit.API.Data;
using GymFit.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using GymFit.API.Models.Requests;
namespace GymFit.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]  // Doar admin poate folosi acest controller
    public class ClientsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public ClientsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: api/clients
        [HttpGet]
        public async Task<IActionResult> GetClients()
        {
            var clients = await _db.Users
                .Where(u => u.Role == "Client")
                .Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.Email
                })
                .ToListAsync();

            return Ok(clients);
        }

        // GET: api/clients/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetClient(int id)
        {
            var client = await _db.Users
                .Where(u => u.Role == "Client" && u.Id == id)
                .Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.Email
                })
                .FirstOrDefaultAsync();

            if (client == null)
                return NotFound();

            return Ok(client);
        }

        // POST: api/clients
       [HttpPost]
public async Task<IActionResult> CreateClient([FromBody] CreateClientRequest request)
{
    if (string.IsNullOrWhiteSpace(request.Email) ||
        string.IsNullOrWhiteSpace(request.FullName) ||
        string.IsNullOrWhiteSpace(request.Password))
    {
        return BadRequest("FullName, Email și Password sunt obligatorii.");
    }

    if (await _db.Users.AnyAsync(u => u.Email == request.Email))
    {
        return BadRequest("Email deja folosit.");
    }

    var hashedPassword = HashPassword(request.Password);

    var client = new AppUser
    {
        FullName = request.FullName,
        Email = request.Email,
        PasswordHash = hashedPassword,
        Role = "Client"
    };

    _db.Users.Add(client);
    await _db.SaveChangesAsync();

    return CreatedAtAction(nameof(GetClient), new { id = client.Id }, new
    {
        client.Id,
        client.FullName,
        client.Email
    });
}


        // PUT: api/clients/5
        [HttpPut("{id}")]
public async Task<IActionResult> UpdateClient(int id, [FromBody] UpdateClientRequest updatedClient)
{
    var client = await _db.Users.FindAsync(id);
    if (client == null || client.Role != "Client")
        return NotFound();

    client.FullName = updatedClient.FullName;
    client.Email = updatedClient.Email;

    // Actualizare rol
    if (!string.IsNullOrEmpty(updatedClient.Role))
    {
        client.Role = updatedClient.Role;
    }

    await _db.SaveChangesAsync();
    return NoContent();
}


        // DELETE: api/clients/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClient(int id)
        {
            var client = await _db.Users.FindAsync(id);
            if (client == null || client.Role != "Client")
                return NotFound();

            _db.Users.Remove(client);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        // HashPassword cu SHA256 identic cu cel din AuthController
      
        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

    }
}
