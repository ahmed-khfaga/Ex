using ExaminationSystemTT.DAL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExaminationSystemTT.BLL.Interfaces
{
    public interface IStudentAnswerRepository
    {
        /// <summary>
        /// Adds a new student answer or updates an existing one for the same student, exam, and question.
        /// </summary>
        /// <param name="answer">The StudentAnswer object containing the details.</param>
        /// <returns>The number of state entries written to the database.</returns>
        Task<int> AddOrUpdateAnswerAsync(StudentAnswer answer);

        /// <summary>
        /// Gets a specific answer submitted by a student for a specific question within a specific exam.
        /// </summary>
        /// <param name="studentId">The ID of the student.</param>
        /// <param name="examId">The ID of the exam.</param>
        /// <param name="questionId">The ID of the question.</param>
        /// <returns>The StudentAnswer object or null if not found.</returns>
        Task<StudentAnswer?> GetAnswerAsync(int studentId, int examId, int questionId);

        /// <summary>
        /// Gets all answers submitted by a specific student for a specific exam.
        /// </summary>
        /// <param name="studentId">The ID of the student.</param>
        /// <param name="examId">The ID of the exam.</param>
        /// <returns>An enumerable collection of StudentAnswer objects.</returns>
        Task<IEnumerable<StudentAnswer>> GetExamAnswersAsync(int studentId, int examId);

        // You might add other methods later if needed, e.g.:
        // Task<int> DeleteAnswerAsync(int studentAnswerId);
        // Task<IEnumerable<StudentAnswer>> GetAllAnswersForExamAsync(int examId); // For grading/review
    }
}