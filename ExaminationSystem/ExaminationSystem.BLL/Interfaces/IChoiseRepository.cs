using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExaminationSystem.DAL.Models;

namespace ExaminationSystem.BLL.Interfaces
{
    public interface IChoiseRepository
    {
        IEnumerable<Choice> GetAll(); 
        Choice GetByID(int id); 
        int Add(Choice choice);
        int Update(Choice choice); 
        int Delete(Choice choice);
        IEnumerable<Choice> GetChoicesByQuestionID(int questionId); 
    }
}
