using System.Collections.Generic;
using System.Linq;
using ExaminationSystemTT.BLL.Interfaces;
using ExaminationSystemTT.DAL;
using ExaminationSystemTT.DAL.Data;
using ExaminationSystemTT.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace ExaminationSystemTT.BLL.Repositories
{
    public class QuestionRepository : IQuestionRepository
    {
        private readonly ExaminationContext _dbcontext;

        public QuestionRepository(ExaminationContext context)
        {
            _dbcontext = context;
        }

        public int Add(Question question)
        {
            _dbcontext.Questions.Add(question);
            return _dbcontext.SaveChanges();
        }

        public int Delete(Question question)
        {
            _dbcontext.Questions.Remove(question);
            return _dbcontext.SaveChanges();
        }

        public IEnumerable<Question> GetAll()
        {
           
            // Note: Might want to include Exam info?
            return _dbcontext.Questions.AsNoTracking().ToList();
        }
        public IEnumerable<Question> GetByExamId(int examId)
        {
            return _dbcontext.Questions
                             .Where(q => q.ExamId == examId)
                             .AsNoTracking() // Good for read-only lists
                             .ToList();
        }

        public Question? GetByID(int id)
        {
            return _dbcontext.Questions.Find(id);
        }

        public int Update(Question question)
        {
            _dbcontext.Questions.Update(question);
            return _dbcontext.SaveChanges();
        }

        
    }
}