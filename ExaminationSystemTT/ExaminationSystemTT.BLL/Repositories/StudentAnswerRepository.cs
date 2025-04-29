using ExaminationSystemTT.BLL.Interfaces;
using ExaminationSystemTT.DAL.Data;
using ExaminationSystemTT.DAL.Models;
using Microsoft.EntityFrameworkCore; // Required for EF Core methods like FirstOrDefaultAsync, AddAsync etc.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks; // Required for async Tasks

namespace ExaminationSystemTT.BLL.Repositories
{
    public class StudentAnswerRepository : IStudentAnswerRepository
    {
        private readonly ExaminationContext _context; // Use _context consistently

        public StudentAnswerRepository(ExaminationContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Adds a new student answer or updates an existing one for the same student, exam, and question.
        /// </summary>
        public async Task<int> AddOrUpdateAnswerAsync(StudentAnswer answer)
        {
            if (answer == null)
            {
                throw new ArgumentNullException(nameof(answer));
            }

            // Find if an answer already exists for this specific combination
            var existingAnswer = await _context.StudentAnswers
                .FirstOrDefaultAsync(sa => sa.StudentId == answer.StudentId &&
                                            sa.ExamId == answer.ExamId &&
                                            sa.QuestionId == answer.QuestionId);

            if (existingAnswer != null)
            {
                // Update the existing answer's selected values
                existingAnswer.SelectedOptionIndex = answer.SelectedOptionIndex;
                existingAnswer.SelectedAnswerTF = answer.SelectedAnswerTF;
                // If you add a timestamp, update it here: existingAnswer.AnsweredTimestamp = DateTime.UtcNow;

                _context.StudentAnswers.Update(existingAnswer); // Mark entity as modified
            }
            else
            {
                // Add the new answer record
                // If you add a timestamp, set it here: answer.AnsweredTimestamp = DateTime.UtcNow;
                await _context.StudentAnswers.AddAsync(answer);
            }

            // Save changes to the database
            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Gets a specific answer submitted by a student for a specific question within a specific exam.
        /// </summary>
        public async Task<StudentAnswer?> GetAnswerAsync(int studentId, int examId, int questionId)
        {
            return await _context.StudentAnswers
                .AsNoTracking() // Good for read-only operations
                .FirstOrDefaultAsync(sa => sa.StudentId == studentId &&
                                            sa.ExamId == examId &&
                                            sa.QuestionId == questionId);
        }

        /// <summary>
        /// Gets all answers submitted by a specific student for a specific exam.
        /// </summary>
        public async Task<IEnumerable<StudentAnswer>> GetExamAnswersAsync(int studentId, int examId)
        {
            return await _context.StudentAnswers
                .Where(sa => sa.StudentId == studentId && sa.ExamId == examId)
                .AsNoTracking() // Good for read-only lists
                .ToListAsync(); // Materialize the query to a list
        }
    }
}