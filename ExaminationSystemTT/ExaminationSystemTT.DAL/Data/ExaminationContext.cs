using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExaminationSystemTT.DAL.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ExaminationSystemTT.DAL.Data
{
    public class ExaminationContext : IdentityDbContext<ApplicationUser>
    {

        public ExaminationContext(DbContextOptions<ExaminationContext> options) : base(options)
        {
        }

        public DbSet<Student> Students { get; set; }
        public DbSet<Instructor> Instructors { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Exam> Exams { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<StudentAnswer> StudentAnswers { get; set; }

        public DbSet<ExamAttempt> ExamAttempts { get; set; }


    
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Existing configurations...
            modelBuilder.Entity<StudentAnswer>()
              .HasOne(sa => sa.Question)
              .WithMany(q => q.StudentAnswers)
              .HasForeignKey(sa => sa.QuestionId)
              .OnDelete(DeleteBehavior.NoAction); // Changed from Cascade maybe? Or keep Cascade if answers are exam-specific

            modelBuilder.Entity<StudentAnswer>()
               .HasOne(sa => sa.Exam)
               .WithMany(e => e.StudentAnswers)
               .HasForeignKey(sa => sa.ExamId)
               .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Question>()
               .HasOne(q => q.Exam)
               .WithMany(e => e.Questions)
               .HasForeignKey(q => q.ExamId)
               .OnDelete(DeleteBehavior.Cascade);

            // --- Configure ExamAttempt Relationships ---
            modelBuilder.Entity<ExamAttempt>()
               .HasOne(ea => ea.Student)
               .WithMany() // Assuming Student doesn't need a direct list of attempts
               .HasForeignKey(ea => ea.StudentId)
               .OnDelete(DeleteBehavior.Cascade); // Delete attempts if student is deleted

            modelBuilder.Entity<ExamAttempt>()
                .HasOne(ea => ea.Exam)
                .WithMany() // Assuming Exam doesn't need a direct list of attempts
                .HasForeignKey(ea => ea.ExamId)
                .OnDelete(DeleteBehavior.Cascade); // Delete attempts if exam is deleted

            modelBuilder.Entity<Question>()
               .HasOne(q => q.Exam)
               .WithMany(e => e.Questions)
               .HasForeignKey(q => q.ExamId)
               .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<IdentityRole>()
                .ToTable("Roles");
            modelBuilder.Entity<IdentityUser>()
                .ToTable("Users");
        }
    }
}
