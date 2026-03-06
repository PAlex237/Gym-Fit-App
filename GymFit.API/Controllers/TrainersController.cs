using GymFit.API.Data;
using GymFit.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using GymFit.API.Models.Requests;
namespace GymFit.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrainersController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public TrainersController(ApplicationDbContext db)
        {
            _db = db;
        }

        // Orice utilizator autentificat poate vedea lista antrenorilor
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetTrainers()
        {
            var trainers = await _db.Users
                .Where(u => u.Role == "Trainer")
                .Select(t => new
                {
                    t.Id,
                    t.FullName,
                    t.Email,
                    // alte câmpuri relevante
                })
                .ToListAsync();

            return Ok(trainers);
        }

        // Admin poate crea antrenori


        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
[HttpPost]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> CreateTrainer([FromBody] CreateTrainerRequest request)
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

    var trainer = new AppUser
    {
        FullName = request.FullName,
        Email = request.Email,
        PasswordHash = hashedPassword,
        Role = "Trainer"
    };

    _db.Users.Add(trainer);
    await _db.SaveChangesAsync();

    return CreatedAtAction(nameof(GetTrainers), new { id = trainer.Id }, new
    {
        trainer.Id,
        trainer.FullName,
        trainer.Email
    });
}


        // Admin poate modifica antrenori
       [HttpPut("{id}")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> UpdateTrainer(int id, [FromBody] UpdateTrainerRequest updatedTrainer)
{
    var trainer = await _db.Users.FindAsync(id);
    if (trainer == null || trainer.Role != "Trainer")
        return NotFound();

    trainer.FullName = updatedTrainer.FullName;
    trainer.Email = updatedTrainer.Email;

    if (!string.IsNullOrEmpty(updatedTrainer.Role))
    {
        trainer.Role = updatedTrainer.Role;
    }

    await _db.SaveChangesAsync();
    return NoContent();
}


        // Admin poate șterge antrenori
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTrainer(int id)
        {
            var trainer = await _db.Users.FindAsync(id);
            if (trainer == null || trainer.Role != "Trainer")
                return NotFound();

            _db.Users.Remove(trainer);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
