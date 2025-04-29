using ExaminationSystemTT.DAL.Models;

namespace ExaminationSystemTT.PL.ViewModels
{
    public class ExamTakingViewModel
    {
        public Exam Exam { get; set; }
        public List<Question> Questions { get; set; } // Or maybe one question at a time
        public Dictionary<int, StudentAnswer> ExistingAnswers { get; set; } // To show previous answers
                                                                            // Add properties to hold the current answer being submitted
        public int CurrentQuestionId { get; set; }
        public int? SelectedOptionIndex { get; set; } // For MCQ
        public bool? SelectedAnswerTF { get; set; }  // For TF
    }
}
