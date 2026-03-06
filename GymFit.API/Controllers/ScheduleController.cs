using GymFit.API.Data;
using GymFit.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GymFit.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Client")] // doar clientul poate face programări
    public class ScheduleController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public ScheduleController(ApplicationDbContext db)
        {
            _db = db;
        }

        // Model simplu pentru cererea de creare programare
        public class AddScheduleRequest
        {
            public int CourseId { get; set; }
        }

        [HttpGet]
        public async Task<IActionResult> GetMySchedules()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var schedules = await _db.Schedules
                .Include(s => s.Course)
                .Where(s => s.ClientId == userId)
                .Select(s => new
                {
                    s.Id,
                    s.ScheduledAt,
                    Course = new { s.Course.Id, s.Course.Name, s.Course.StartDate, s.Course.EndDate }
                })
                .ToListAsync();

            return Ok(schedules);
        }

        [HttpPost]
        public async Task<IActionResult> AddSchedule([FromBody] AddScheduleRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Verifică dacă există cursul
            var course = await _db.Courses.FindAsync(request.CourseId);
            if (course == null)
                return NotFound("Course not found.");

            // Verifică dacă clientul nu s-a înscris deja la curs
            bool alreadyScheduled = await _db.Schedules
                .AnyAsync(s => s.ClientId == userId && s.CourseId == request.CourseId);

            if (alreadyScheduled)
                return BadRequest("You are already scheduled for this course.");

            // Optional: Verifică dacă cursul încă nu s-a terminat
            if (course.EndDate < DateTime.UtcNow)
                return BadRequest("Cannot schedule for a course that has already ended.");

            var schedule = new Schedule
            {
                ClientId = userId,
                CourseId = request.CourseId,
                ScheduledAt = DateTime.UtcNow // sau altă logică pentru data programării
            };

            _db.Schedules.Add(schedule);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMySchedules), new { id = schedule.Id }, schedule);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var schedule = await _db.Schedules.FindAsync(id);

            if (schedule == null || schedule.ClientId != userId)
                return NotFound();

            _db.Schedules.Remove(schedule);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}
