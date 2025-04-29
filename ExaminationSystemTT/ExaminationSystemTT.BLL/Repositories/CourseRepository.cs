using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExaminationSystemTT.BLL.Interfaces;
using ExaminationSystemTT.DAL.Data;
using ExaminationSystemTT.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace ExaminationSystemTT.BLL.Repositories
{
    public class CourseRepository : ICourseRepository
    {
        private readonly ExaminationContext _dbcontext;

        public CourseRepository(ExaminationContext context)
        {
            _dbcontext = context;
        }
        public int Add(Course course)
        {
            _dbcontext.Courses.Add(course);
            return _dbcontext.SaveChanges();
        }

        public int Delete(Course course)
        {
            _dbcontext.Courses.Remove(course);
            return _dbcontext.SaveChanges();
        }

        public IEnumerable<Course> GetAll()
        {
            return _dbcontext.Courses.AsNoTracking().ToList();
        }

        public Course GetByID(int id)
        {
            return _dbcontext.Courses.Find(id);
        }

        public int Update(Course course)
        {
            _dbcontext.Courses.Update(course);
            return _dbcontext.SaveChanges();
        }
    }
}
