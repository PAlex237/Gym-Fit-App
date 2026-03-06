using Microsoft.EntityFrameworkCore;
using GymFit.API.Models;

namespace GymFit.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) {}

        public DbSet<AppUser> Users { get; set; }   // Am schimbat Users -> AppUsers
        public DbSet<Course> Courses { get; set; }
        public DbSet<Schedule> Schedules { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Relație 1 Trainer - Multe Course (Trainer opțional)
            modelBuilder.Entity<Course>()
                .HasOne(c => c.Trainer)
                .WithMany(t => t.Courses)  // AppUser (Trainer) are ICollection<Course> Courses
                .HasForeignKey(c => c.TrainerId)
                .OnDelete(DeleteBehavior.SetNull);

            // Relație 1 Client - Multe Schedule
            modelBuilder.Entity<Schedule>()
                .HasOne(s => s.Client)
                .WithMany(c => c.Schedules)  // AppUser (Client) are ICollection<Schedule> Schedules
                .HasForeignKey(s => s.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relație 1 Course - Multe Schedule
            modelBuilder.Entity<Schedule>()
                .HasOne(s => s.Course)
                .WithMany(c => c.Schedules)
                .HasForeignKey(s => s.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
