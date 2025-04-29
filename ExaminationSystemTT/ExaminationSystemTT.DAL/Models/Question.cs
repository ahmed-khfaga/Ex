using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ExaminationSystemTT.DAL.Models
{
    public class Question
    {
        [Key]
        public int QuestionId { get; set; }

        // Foreign Key for Exam
        [Required]
        public int ExamId { get; set; }

        [ForeignKey("ExamId")]
        [ValidateNever]
        public virtual Exam Exam { get; set; }

        [Required(ErrorMessage = "Question text is required.")]
        public string QuestionText { get; set; } // Maps to NVARCHAR(MAX) or TEXT

        [Required(ErrorMessage = "Question type is required.")]
        [StringLength(20)] // Example: "MCQ", "TF"
        public string QuestionType { get; set; } // Using string

        [Required(ErrorMessage = "Mark value is required.")]
        [Range(1, 100, ErrorMessage = "Mark must be between 1 and 100.")] // Example validation
        public int Mark { get; set; }

        // --- MCQ Specific Fields ---
        [StringLength(500)]
        public string? Option1 { get; set; } // Nullable

        [StringLength(500)]
        public string? Option2 { get; set; } // Nullable

        [StringLength(500)]
        public string? Option3 { get; set; } // Nullable

        [StringLength(500)]
        public string? Option4 { get; set; } // Nullable

        public int? CorrectOptionIndex { get; set; } // Nullable (1-4 for MCQ)

        // --- True/False Specific Field ---
        public bool? CorrectAnswerTF { get; set; } // Nullable (true/false for TF)

        // Navigation property
        [ValidateNever]
        public virtual ICollection<StudentAnswer> StudentAnswers { get; set; } = new HashSet<StudentAnswer>();
    }
}