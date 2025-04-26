using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;

namespace ExaminationSystem.PL.ViewModels
{
    public class ChoiceViewModel
    {
        // No ID needed for create, maybe for edit later
        [Required(ErrorMessage = "Choice text is required for MCQ.")]
        [StringLength(500)]
        public string ChoiceText { get; set; } = string.Empty;
        public bool IsCorrect { get; set; } = false;
    }
    public class QuestionViewModel
    {
        [Key]
        public int ID { get; set; } // Needed for Edit

        [Required]
        [Display(Name = "Question Text")]
        public string QuestionText { get; set; } = string.Empty;

        [Required]
        [Range(1, 100, ErrorMessage = "Mark must be between 1 and 100.")]
        public int Mark { get; set; }

        [Required]
        [Display(Name = "Question Type")]
        public string QuestionType { get; set; } = "MCQ"; // Default to MCQ

        [Required(ErrorMessage = "Please select the Exam.")]
        [Display(Name = "Exam")]
        public int ExamID { get; set; } // Foreign Key

        // --- TF Specific ---
        [Display(Name = "Correct Answer (True/False)")]
        // Required only if QuestionType is TF (custom validation might be needed)
        public bool? CorrectTFAnswer { get; set; }

        // --- MCQ Specific ---
        [Display(Name = "Choices (for MCQ)")]
        // Needs validation: at least 2 choices, at least 1 correct if MCQ
        public List<ChoiceViewModel> Choices { get; set; }

        // --- For Dropdown ---
        public SelectList? ExamList { get; set; }

        // Constructor to initialize choices list
        public QuestionViewModel()
        {
            Choices = new List<ChoiceViewModel>()
            {
                new ChoiceViewModel(), // Start with a few empty slots for choices
                new ChoiceViewModel(),
                new ChoiceViewModel(),
                new ChoiceViewModel()
            };
        }
    }
}
