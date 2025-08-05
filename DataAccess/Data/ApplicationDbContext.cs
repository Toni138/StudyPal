using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyModels;

namespace DataAccess.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<UserStats> UserStats { get; set; }

        public DbSet<StudySession> StudySession { get; set; }

        public DbSet<SubjectStudyHours> SubjectStudyHours { get; set; }
        public DbSet<Flashcard> Flashcards { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasOne(u => u.UserStats)
                .WithOne(us => us.User)
                .HasForeignKey<UserStats>(us => us.UserId);

            modelBuilder.Entity<UserStats>()
                .HasKey(us => us.Id);

            modelBuilder.Entity<SubjectStudyHours>()
                .HasOne(s => s.UserStats)
                .WithMany(us => us.HoursPerSubject)
                .HasForeignKey(s => s.UserStatsId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<StudySession>()
                .HasOne(ss => ss.User)
                .WithMany(u => u.StudySessions)
                .HasForeignKey(ss => ss.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }

    }
}
