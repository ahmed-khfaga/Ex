using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExaminationSystem.BLL.Interfaces;
using ExaminationSystem.DAL.Data;
using ExaminationSystem.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace ExaminationSystem.BLL.Repositories
{
    public class ExamRepository : IExamRepository
    {
        private readonly ExaminitionSystemDbContext _dbcontext;

        public ExamRepository(ExaminitionSystemDbContext dbcontext)
        {
            _dbcontext = dbcontext;
        }
        public int Add(Exam exam)
        {
            _dbcontext.Exams.Add(exam); // state added 
            return _dbcontext.SaveChanges();
        }

        public int Delete(Exam exam)
        {
            _dbcontext.Exams.Remove(exam); // state added 
            return _dbcontext.SaveChanges();
        }

        public IEnumerable<Exam> GetAll()
        {
            return _dbcontext.Exams.Include(e => e.Course).Include(e => e.Instructor).AsNoTracking().ToList();

        }

        public Exam GetByID(int id)
        {
            var exam =  _dbcontext.Exams
                         .Include(e => e.Course)
                         .Include(e => e.Instructor)
                         .FirstOrDefault(e => e.ID == id);
            return exam;
        }

        public int Update(Exam exam)
        {
            _dbcontext.Exams.Update(exam); // state added 
            return _dbcontext.SaveChanges();
        }
    }
}
