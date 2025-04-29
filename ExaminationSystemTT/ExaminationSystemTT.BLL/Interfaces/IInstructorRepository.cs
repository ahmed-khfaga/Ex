using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExaminationSystemTT.DAL.Models;

namespace ExaminationSystemTT.BLL.Interfaces
{
    public interface IInstructorRepository
    {

        IEnumerable<Instructor> GetAll();
        Instructor GetByID(int id);
        int Add(Instructor instructor);
        int Update(Instructor instructor);
        int Delete(Instructor instructor);
    }
}
