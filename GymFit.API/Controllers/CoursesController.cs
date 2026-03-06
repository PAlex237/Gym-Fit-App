using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using GymFit.API.Data;
using GymFit.API.Models;

namespace GymFit.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoursesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public CoursesController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: api/courses
        [HttpGet]
        public async Task<IActionResult> GetCourses()
        {
            var courses = await _db.Courses
                .Include(c => c.Trainer)
                .ToListAsync();

            return Ok(courses.Select(c => new
            {
                c.Id,
                c.Name,
                c.Description,
                c.StartDate,
                c.EndDate,
                Trainer = c.Trainer == null ? null : new { c.Trainer.Id, c.Trainer.FullName }
            }));
        }

        // GET: api/courses/trainers
        [HttpGet("trainers")]
        public async Task<IActionResult> GetTrainers()
        {
            var trainers = await _db.Users
                .Where(u => u.Role == "Trainer")
                .Select(t => new { t.Id, t.FullName, t.Email })
                .ToListAsync();

            return Ok(trainers);
        }

        // GET: api/courses/my-schedule
        [HttpGet("my-schedule")]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> GetMySchedule()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var schedules = await _db.Schedules
                .Where(s => s.ClientId == userId)
                .Include(s => s.Course)
                    .ThenInclude(c => c.Trainer)
                .ToListAsync();

            return Ok(schedules.Select(s => new
            {
                s.Course.Id,
                s.Course.Name,
                s.Course.Description,
                s.Course.StartDate,
                s.Course.EndDate,
                Trainer = s.Course.Trainer == null ? null : new { s.Course.Trainer.Id, s.Course.Trainer.FullName },
                s.ScheduledAt
            }));
        }

        // GET: api/courses/available
        // Lista cursurilor la care clientul nu este înscris încă
        [HttpGet("available")]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> GetAvailableCourses()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var enrolledCourseIds = await _db.Schedules
                .Where(s => s.ClientId == userId)
                .Select(s => s.CourseId)
                .ToListAsync();

            var availableCourses = await _db.Courses
                .Where(c => !enrolledCourseIds.Contains(c.Id))
                .Include(c => c.Trainer)
                .ToListAsync();

            return Ok(availableCourses.Select(c => new
            {
                c.Id,
                c.Name,
                c.Description,
                c.StartDate,
                c.EndDate,
                Trainer = c.Trainer == null ? null : new { c.Trainer.Id, c.Trainer.FullName }
            }));
        }

        // POST: api/courses/enroll/5
        [HttpPost("enroll/{courseId}")]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> EnrollInCourse(int courseId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            bool alreadyEnrolled = await _db.Schedules.AnyAsync(s => s.ClientId == userId && s.CourseId == courseId);
            if (alreadyEnrolled)
                return BadRequest("Already enrolled in this course.");

            var schedule = new Schedule
            {
                ClientId = userId,
                CourseId = courseId,
                ScheduledAt = DateTime.UtcNow
            };

            _db.Schedules.Add(schedule);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/courses/unenroll/5
        [HttpDelete("unenroll/{courseId}")]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> UnenrollFromCourse(int courseId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var schedule = await _db.Schedules
                .FirstOrDefaultAsync(s => s.ClientId == userId && s.CourseId == courseId);

            if (schedule == null)
                return NotFound();

            _db.Schedules.Remove(schedule);
            await _db.SaveChangesAsync();

            return NoContent();
        }
// POST: api/courses
[HttpPost]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> CreateCourse([FromBody] Course course)
{
    if (course == null)
        return BadRequest();

    _db.Courses.Add(course);
    await _db.SaveChangesAsync();

    return CreatedAtAction(nameof(GetCourses), new { id = course.Id }, course);
}

// PUT: api/courses/{id}
[HttpPut("{id}")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> UpdateCourse(int id, [FromBody] Course updatedCourse)
{
    if (id != updatedCourse.Id)
        return BadRequest("ID mismatch");

    var course = await _db.Courses.FindAsync(id);
    if (course == null)
        return NotFound();

    course.Name = updatedCourse.Name;
    course.Description = updatedCourse.Description;
    course.StartDate = updatedCourse.StartDate;
    course.EndDate = updatedCourse.EndDate;
    course.TrainerId = updatedCourse.TrainerId;

    await _db.SaveChangesAsync();

    return NoContent();
}

// DELETE: api/courses/{id}
[HttpDelete("{id}")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> DeleteCourse(int id)
{
    var course = await _db.Courses.FindAsync(id);
    if (course == null)
        return NotFound();

    _db.Courses.Remove(course);
    await _db.SaveChangesAsync();

    return NoContent();
}

    }
}
