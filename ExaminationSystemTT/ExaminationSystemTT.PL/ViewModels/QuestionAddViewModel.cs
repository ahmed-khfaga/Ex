using ExaminationSystemTT.DAL.Models;
using System.ComponentModel.DataAnnotations;

namespace ExaminationSystemTT.PL.ViewModels
{
    public class QuestionAddViewModel
    {
        public Question Question { get; set; } = new Question();

        // Store Exam info for context in the view
        public int ExamId { get; set; } // Ensure this is set when creating the VM
        public string? ExamCourseName { get; set; } // For display
    }
}
