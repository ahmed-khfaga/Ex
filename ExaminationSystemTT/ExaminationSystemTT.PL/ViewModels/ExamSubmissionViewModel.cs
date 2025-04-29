using System.ComponentModel.DataAnnotations;

namespace ExaminationSystemTT.PL.ViewModels
{
    public class ExamSubmissionViewModel
    {
        [Required]
        public int ExamId { get; set; }

        // This will bind to the name="Answers[index].PropertyName" inputs
        public List<AnswerSubmission> Answers { get; set; } = new List<AnswerSubmission>();
    }
}
