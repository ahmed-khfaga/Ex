using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExaminationSystemTT.DAL.Models;

namespace ExaminationSystemTT.BLL.Interfaces
{
    public interface IStudentRepository
    {
        IEnumerable<Student> GetAll();
        Student GetByID(int id);
        int Add(Student student);
        int Update(Student student);
        int Delete(Student student);
    }
}
