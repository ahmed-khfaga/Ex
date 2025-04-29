namespace ExaminationSystemTT.PL.ViewModels
{
    public class AnswerSubmission
    {
        // Needs to match hidden input name="Answers[index].QuestionId"
        public int QuestionId { get; set; }

        // Needs to match radio button name="Answers[index].SelectedOptionIndex"
        public int? SelectedOptionIndex { get; set; } // For MCQ

        // Needs to match radio button name="Answers[index].SelectedAnswerTF"
        public bool? SelectedAnswerTF { get; set; }  // For TF
    }
}