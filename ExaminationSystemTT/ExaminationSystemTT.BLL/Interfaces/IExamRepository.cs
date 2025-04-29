using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExaminationSystemTT.DAL.Models;

namespace ExaminationSystemTT.BLL.Interfaces
{
    public interface IExamRepository
    {
        IEnumerable<Exam> GetAll();
        IEnumerable<Exam> GetAllWithCourseAndInstructor();
        Exam? GetByIdWithDetails(int id);
        Exam GetByID(int id);
        int Add(Exam exam);
        int Update(Exam exam);
        int Delete(Exam exam);
    }
}
