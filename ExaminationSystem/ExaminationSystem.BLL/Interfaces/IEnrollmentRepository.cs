using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExaminationSystem.DAL.Models;

namespace ExaminationSystem.BLL.Interfaces
{
    public interface IEnrollmentRepository
    {
        IEnumerable<Enrollment> GetAll();
        bool IsStudentEnrolled(int studentId, int courseId);

        IEnumerable<Enrollment> GetEnrollmentsByStudentID(int studentId);
        int Add(Enrollment enrollment);
        int Delete(int studentId, int courseId); // Alternative delete signature

    }
}
