using ExaminationSystemTT.DAL.Models;
using System.Collections.Generic;

// Ensure the namespace matches your project structure
namespace ExaminationSystemTT.PL.ViewModels
{
    public class ExamReviewViewModel
    {
        public Exam Exam { get; set; }
        public ExamAttempt AttemptDetails { get; set; }
        // Use Dictionary for easy lookup in the view
        public Dictionary<int, StudentAnswer> SubmittedAnswers { get; set; } = new Dictionary<int, StudentAnswer>();

        // Questions can be accessed via Exam.Questions
    }
}