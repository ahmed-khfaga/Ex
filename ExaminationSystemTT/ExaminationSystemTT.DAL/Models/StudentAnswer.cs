using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExaminationSystemTT.DAL.Models
{
    public class StudentAnswer
    {
        [Key]
        public int StudentAnswerId { get; set; }

        // Foreign Key for Student
        [Required]
        public int StudentId { get; set; }

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        // Foreign Key for Exam
        [Required]
        public int ExamId { get; set; }

        [ForeignKey("ExamId")]
        public virtual Exam Exam { get; set; }

        // Foreign Key for Question
        [Required]
        public int QuestionId { get; set; }

        [ForeignKey("QuestionId")]
        public virtual Question Question { get; set; }

        // --- Student's Answer ---
        public int? SelectedOptionIndex { get; set; } // Nullable (1-4 for MCQ answer)
        public bool? SelectedAnswerTF { get; set; } // Nullable (true/false for TF answer)

        // Consider adding a Timestamp field if needed:
        // public DateTime AnsweredTimestamp { get; set; } = DateTime.UtcNow;
    }
}