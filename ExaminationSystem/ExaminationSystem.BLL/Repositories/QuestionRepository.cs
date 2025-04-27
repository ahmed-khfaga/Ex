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
    public class QuestionRepository : IQuestionRepository
    {
        private readonly ExaminitionSystemDbContext _dbcontext;

        public QuestionRepository(ExaminitionSystemDbContext dbcontext)
        {
            _dbcontext = dbcontext;
        }
        public int Add(Question question)
        {
            _dbcontext.Questions.Add(question); // state added 
            return _dbcontext.SaveChanges();
        }

        public int Delete(Question question)
        {
            _dbcontext.Questions.Remove(question); // state added 
            return _dbcontext.SaveChanges();
        }

        public IEnumerable<Question> GetAll()
        {
            return _dbcontext.Questions
                      .Include(q => q.Exam) // <-- IMPORTANT: Load the Exam navigation property
                      .AsNoTracking()
                      .ToList();
        }

        public Question GetByID(int id)
        {
            return _dbcontext.Questions.Find(id); // Safer
        }

        public int Update(Question question)
        {
            _dbcontext.Questions.Update(question); // state added 
            return _dbcontext.SaveChanges();
        }
    }
}
