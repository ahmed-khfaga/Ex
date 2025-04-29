using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExaminationSystemTT.DAL.Models;

namespace ExaminationSystemTT.BLL.Interfaces
{
    public interface IQuestionRepository
    {
        IEnumerable<Question> GetAll();
        IEnumerable<Question> GetByExamId(int examId);

        Question GetByID(int id);
        int Add(Question question);
        int Update(Question question);
        int Delete(Question question);
    }
}
