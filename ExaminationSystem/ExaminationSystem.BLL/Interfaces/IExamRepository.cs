using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExaminationSystem.DAL.Models;

namespace ExaminationSystem.BLL.Interfaces
{
    public interface IExamRepository
    {
        IEnumerable<Exam> GetAll();
        Exam GetByID(int id);
        int Add(Exam exam);
        int Update(Exam exam);
        int Delete(Exam exam);

    }
}
