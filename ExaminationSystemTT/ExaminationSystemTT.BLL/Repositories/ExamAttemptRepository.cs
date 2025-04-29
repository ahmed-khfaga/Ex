using ExaminationSystemTT.BLL.Interfaces;
using ExaminationSystemTT.DAL.Data;
using ExaminationSystemTT.DAL.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ExaminationSystemTT.BLL.Repositories
{
    public class ExamAttemptRepository : IExamAttemptRepository
    {
        private readonly ExaminationContext _context;

        public ExamAttemptRepository(ExaminationContext context)
        {
            _context = context;
        }

        public async Task<int> AddAttemptAsync(ExamAttempt attempt)
        {
            await _context.ExamAttempts.AddAsync(attempt);
            return await _context.SaveChangesAsync();
        }

        public async Task<ExamAttempt?> GetAttemptAsync(int studentId, int examId)
        {
            // Get the most recent attempt if multiple somehow exist (shouldn't with unique index)
            return await _context.ExamAttempts
                                 .Where(ea => ea.StudentId == studentId && ea.ExamId == examId)
                                 .OrderByDescending(ea => ea.StartTime) // Get latest start time
                                 .FirstOrDefaultAsync();
        }

        public async Task<bool> HasCompletedAttemptAsync(int studentId, int examId)
        {
            return await _context.ExamAttempts
                                 .AnyAsync(ea => ea.StudentId == studentId &&
                                                 ea.ExamId == examId &&
                                                 ea.IsCompleted == true); // Check IsCompleted flag
        }

        public async Task<int> UpdateAttemptAsync(ExamAttempt attempt)
        {
            // Ensure EF Core tracks the entity if it wasn't fetched in the same context scope
            _context.Entry(attempt).State = EntityState.Modified;
            return await _context.SaveChangesAsync();
        }
    }
}