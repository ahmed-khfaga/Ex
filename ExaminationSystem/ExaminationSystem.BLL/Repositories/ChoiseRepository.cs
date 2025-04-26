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
    public class ChoiseRepository : IChoiseRepository
    {
        private readonly ExaminitionSystemDbContext _dbcontext;

        public ChoiseRepository(ExaminitionSystemDbContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public int Add(Choice choice)
        {
            _dbcontext.Choices.Add(choice);
            return _dbcontext.SaveChanges();
        }

        public int Delete(Choice choice)
        {
            _dbcontext.Choices.Remove(choice);
            return _dbcontext.SaveChanges();
        }

        public IEnumerable<Choice> GetAll()
        {
            return _dbcontext.Choices
                            .AsNoTracking()
                            .ToList();
        }

        public Choice GetByID(int id)
        {
            return _dbcontext.Choices.Find(id);
        }

        public IEnumerable<Choice> GetChoicesByQuestionID(int questionId)
        {
            return _dbcontext.Choices
                            .Where(c => c.QuestionID == questionId)
                            .AsNoTracking()
                            .ToList();
        }

        public int Update(Choice choice)
        {
            _dbcontext.Choices.Update(choice);
            return _dbcontext.SaveChanges();
        }
    }
}
