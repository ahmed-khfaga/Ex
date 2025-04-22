using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExaminationSystem.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace ExaminationSystem.DAL.Data
{
    public class ExaminitionSystemDbContext :DbContext
    {
        public ExaminitionSystemDbContext(DbContextOptions<ExaminitionSystemDbContext> options) : base(options)
        {
        }

        public virtual DbSet<Instructor> Instructors { get; set; }
        public virtual DbSet<Student> Students { get; set; }
        public virtual DbSet<Course> Courses { get; set; }
        public virtual DbSet<Enrollment> Enrollments { get; set; }
        public virtual DbSet<Exam> Exams { get; set; }
        public virtual DbSet<Question> Questions { get; set; }
        public virtual DbSet<Choice> Choices { get; set; }
        public virtual DbSet<Submission> Submissions { get; set; }
        public virtual DbSet<Answer> Answers { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Enrollment>()
               .HasKey(e => new { e.Student_ID, e.Course_ID }); // Define composite key



            modelBuilder.Entity<Enrollment>()
               .HasOne(en => en.Student)
               .WithMany(s => s.Enrollments)
               .HasForeignKey(en => en.Student_ID)
               .OnDelete(DeleteBehavior.Restrict); // Prevent deleting Student if enrolled


            modelBuilder.Entity<Enrollment>()
                .HasOne(en => en.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(en => en.Course_ID)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deleting Course if students enrolled

            // Course relationships
            modelBuilder.Entity<Course>()
               .HasOne(c => c.Instructor)
               .WithMany(i => i.Courses)
               .HasForeignKey(c => c.InstructorID)
               .OnDelete(DeleteBehavior.Restrict); // Prevent deleting Instructor if they teach


            // Exam relationships
            modelBuilder.Entity<Exam>()
                .HasOne(e => e.Course)
                .WithMany(c => c.Exams)
                .HasForeignKey(e => e.CourseID)
                .OnDelete(DeleteBehavior.Cascade); // Delete exams if course is deleted

            modelBuilder.Entity<Exam>()
                .HasOne(e => e.Instructor)
                .WithMany(i => i.ExamsCreated) // Changed from Courses to ExamsCreated
                .HasForeignKey(e => e.InstructorID)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deleting Instructor if they created

            // Question relationships
            modelBuilder.Entity<Question>()
                .HasOne(q => q.Exam)
                .WithMany(e => e.Questions)
                .HasForeignKey(q => q.ExamID)
                .OnDelete(DeleteBehavior.Cascade); // Delete questions if exam is deleted

            // Choice relationships
            modelBuilder.Entity<Choice>()
                .HasOne(ch => ch.Question)
                .WithMany(q => q.Choices)
                .HasForeignKey(ch => ch.QuestionID)
                .OnDelete(DeleteBehavior.Cascade); // Delete choices if question is deleted

            
            // Submission relationships
            modelBuilder.Entity<Submission>()
                .HasOne(sub => sub.student)
                .WithMany(s => s.Submissions)
                .HasForeignKey(sub => sub.Student_ID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Submission>()
               .HasOne(sub => sub.exam)
               .WithMany(e => e.Submissions)
               .HasForeignKey(sub => sub.Exam_ID)
               .OnDelete(DeleteBehavior.Cascade); // Usually delete submissions if exam is deleted
           
            
            modelBuilder.Entity<Answer>()
                .HasOne(a => a.submission)
                .WithMany(s => s.Answers)
                .HasForeignKey(a => a.Subm_ID)
                .OnDelete(DeleteBehavior.Cascade); // Delete answers if submission is deleted

            modelBuilder.Entity<Answer>()
                .HasOne(a => a.question)
                .WithMany(q => q.Answers)
                .HasForeignKey(a => a.QuestionID)
                .OnDelete(DeleteBehavior.Restrict); // IMPORTANT: Don't delete Question just because answer exists

            modelBuilder.Entity<Answer>()
                .HasOne(a => a.selectedChoice)
                .WithMany(ch => ch.SelectedByAnswers) // Assumes ICollection<Answer> SelectedByAnswers exists in Choice
                .HasForeignKey(a => a.SelectedChoiceID)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deleting Choice if an answer selected it

            // --- Configure Indexes (Example for Unique Emails) ---
            modelBuilder.Entity<Student>()
                .HasIndex(s => s.Email)
                .IsUnique();
            modelBuilder.Entity<Instructor>()
                .HasIndex(i => i.Email)
                .IsUnique();

        }
    }
}
