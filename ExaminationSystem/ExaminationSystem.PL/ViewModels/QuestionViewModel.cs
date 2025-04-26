using Microsoft.AspNetCore.Mvc.Rendering; // For SelectList if you choose to include it
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ExaminationSystem.PL.ViewModels
{
    // Represents a single choice option within the QuestionViewModel for MCQ
    public class ChoiceViewModel
    {
        // Note: No 'Id' property is present based on your provided code.
        // This necessitates a remove-then-add strategy for editing choices.

        [Required(ErrorMessage = "Choice text cannot be empty if provided.")] // Makes more sense than requiring it always
        [StringLength(500, ErrorMessage = "Choice text cannot exceed 500 characters.")]
        [Display(Name = "Choice Text")]
        public string ChoiceText { get; set; } = string.Empty; // Default to empty

        [Display(Name = "Is Correct?")]
        public bool IsCorrect { get; set; } = false; // Default to false
    }

    // Represents a Question for Create and Edit views
    public class QuestionViewModel
    {
        [Key] // Indicates this is the key, primarily for clarity/metadata
        public int Id { get; set; } // Changed from ID to Id for C# convention

        [Required(ErrorMessage = "Question text is required.")]
        [StringLength(2000, ErrorMessage = "Question text cannot exceed 2000 characters.")] // Added length limit
        [Display(Name = "Question Text")]
        public string QuestionText { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mark value is required.")]
        [Range(1, 100, ErrorMessage = "Mark must be between 1 and 100.")]
        [Display(Name = "Mark")]
        public int Mark { get; set; } = 1; // Default Mark to 1

        [Required(ErrorMessage = "Question type must be selected.")]
        [Display(Name = "Question Type")]
        public string QuestionType { get; set; } = "MCQ"; // Default to MCQ

        [Required(ErrorMessage = "An Exam must be selected.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Exam.")] // Ensure a positive ID is selected
        [Display(Name = "Exam")]
        public int ExamID { get; set; } // Foreign Key

        // --- TF Specific ---
        [Display(Name = "Correct Answer (True/False)")]
        // Note: Actual requirement enforced in Controller based on QuestionType
        public bool? CorrectTFAnswer { get; set; }

        // --- MCQ Specific ---
        [Display(Name = "Choices (for MCQ)")]
        // Note: List-based validation (min 2 choices, at least 1 correct) enforced in Controller
        public List<ChoiceViewModel> Choices { get; set; }

        // --- For Dropdown ---
        // Often populated via ViewBag in the Controller, but can be included here.
        // Make sure it's populated correctly before returning the View from the Controller.
        // Mark as nullable if not always present.
        // [ValidateNever] // Add if necessary, although ViewModels often skip complex validation
        public SelectList? ExamList { get; set; }

        // Constructor to initialize choices list for the Create form UI
        public QuestionViewModel()
        {
            Choices = new List<ChoiceViewModel>()
            {
                // Start with a reasonable number of empty slots for choices in the UI
                new ChoiceViewModel(),
                new ChoiceViewModel(),
                new ChoiceViewModel(),
                new ChoiceViewModel()
                // Add more or fewer as desired for the default create view
            };
        }
    }
}