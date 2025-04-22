using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExaminationSystem.DAL.Models;

namespace ExaminationSystem.BLL.Interfaces
{
    public interface IQuestionRepository
    {
        IEnumerable<Question> GetAll();
        Question GetByID(int id);
        int Add(Question question);
        int Update(Question question);
        int Delete(Question question);
    }
}
