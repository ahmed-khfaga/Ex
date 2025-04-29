using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExaminationSystemTT.BLL.Interfaces;
using ExaminationSystemTT.DAL.Data;
using ExaminationSystemTT.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace ExaminationSystemTT.BLL.Repositories
{
    public class ExamRepository : IExamRepository
    {
        private readonly ExaminationContext _dbcontext;
        public ExamRepository(ExaminationContext context)
        {
            _dbcontext = context;
        }
        public IEnumerable<Exam> GetAllWithCourseAndInstructor()
        {
            return _dbcontext.Exams
                             .Include(e => e.Course)
                             .Include(e => e.Instructor)
                             .AsNoTracking()
                             .ToList(); // Or return Enumerable.Empty if _dbcontext.Exams is null
        }
        public Exam? GetByIdWithDetails(int id)
        {
            return _dbcontext.Exams
                             .Include(e => e.Course)
                             .Include(e => e.Instructor)
                             .Include(e => e.Questions)
                             .FirstOrDefault(e => e.ExamId == id);
        }
        public int Add(Exam exam)
        {
            _dbcontext.Exams.Add(exam);
            return _dbcontext.SaveChanges();
        }

        public int Delete(Exam exam)
        {
            _dbcontext.Exams.Remove(exam);
            return _dbcontext.SaveChanges();
        }

        public IEnumerable<Exam> GetAll()
        {
            return _dbcontext.Exams.AsNoTracking().ToList();
        }

        public Exam GetByID(int id)
        {
            return _dbcontext.Exams.Find(id);
        }

        public int Update(Exam exam)
        {
            _dbcontext.Exams.Update(exam);
            return _dbcontext.SaveChanges();
        }
    }
}
