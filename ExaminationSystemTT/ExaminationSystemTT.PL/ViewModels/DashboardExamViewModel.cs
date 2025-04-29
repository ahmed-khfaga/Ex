// --- Create this ViewModel ---
// File: ViewModels/DashboardExamViewModel.cs
namespace ExaminationSystemTT.PL.ViewModels
{
    public class DashboardExamViewModel
    {
        public ExaminationSystemTT.DAL.Models.Exam Exam { get; set; }
        public bool IsCompleted { get; set; }
    }
}