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
    public class EnrollmentRepository : IEnrollmentRepository
    {
        private readonly ExaminitionSystemDbContext _dbcontext;
        public EnrollmentRepository(ExaminitionSystemDbContext dbcontext)
        {
            _dbcontext = dbcontext;
        }
        public int Add(Enrollment enrollment)
        {
            _dbcontext.Enrollments.Add(enrollment);
            return _dbcontext.SaveChanges();
        }

        public int Delete(int studentId, int courseId)
        {
            var enrollement = _dbcontext.Enrollments.FirstOrDefault(e => e.Student_ID == studentId && e.Course_ID == courseId);
            if (enrollement == null)
                return 0;
            else
            {
                _dbcontext.Enrollments.Remove(enrollement);
                return _dbcontext.SaveChanges();
            }
        }

        public IEnumerable<Enrollment> GetAll()
        {
            return _dbcontext.Enrollments.AsNoTracking().ToList();
        }

        public IEnumerable<Enrollment> GetEnrollmentsByStudentID(int studentId)
        {
            return _dbcontext.Enrollments
                                  .Include(e => e.Course) // Eager load related Course data
                                  .Where(e => e.Student_ID == studentId)
                                  .AsNoTracking() // Use AsNoTracking if data is read-only
                                  .ToList();
        }

        public bool IsStudentEnrolled(int studentId, int courseId)
        {
            return _dbcontext.Enrollments.Any(e =>
                            e.Student_ID == studentId &&
                            e.Course_ID == courseId);
         }
    }
}
