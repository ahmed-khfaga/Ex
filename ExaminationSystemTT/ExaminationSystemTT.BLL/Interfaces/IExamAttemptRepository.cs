using ExaminationSystemTT.DAL.Models;
using System.Threading.Tasks;

namespace ExaminationSystemTT.BLL.Interfaces
{
    public interface IExamAttemptRepository
    {
        /// <summary>
        /// Checks if a student has a completed attempt for a specific exam.
        /// </summary>
        Task<bool> HasCompletedAttemptAsync(int studentId, int examId);

        /// <summary>
        /// Gets an existing attempt (completed or incomplete).
        /// </summary>
        Task<ExamAttempt?> GetAttemptAsync(int studentId, int examId);

        /// <summary>
        /// Adds a new exam attempt record. Typically called when starting an exam.
        /// </summary>
        Task<int> AddAttemptAsync(ExamAttempt attempt);

        /// <summary>
        /// Updates an existing exam attempt record. Typically called upon submission.
        /// </summary>
        Task<int> UpdateAttemptAsync(ExamAttempt attempt);
    }
}