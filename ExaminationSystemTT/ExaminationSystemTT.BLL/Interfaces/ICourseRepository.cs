using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExaminationSystemTT.DAL.Models;

namespace ExaminationSystemTT.BLL.Interfaces
{
    public interface ICourseRepository
    {
        IEnumerable<Course> GetAll();
        Course GetByID(int id);
        int Add(Course course);
        int Update(Course course);
        int Delete(Course course);
    }
}
