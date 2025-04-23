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
    public class StudentRepository : IStudentRepository
    {
        private readonly ExaminitionSystemDbContext _dbcontext;

        public StudentRepository(ExaminitionSystemDbContext dbContext)
        {
            _dbcontext = dbContext;
        }
        public int Add(Student student)
        {
            _dbcontext.Students.Add(student); // state added 
            return _dbcontext.SaveChanges();
        }

        public int Delete(Student student)
        {
            _dbcontext.Students.Remove(student); // state added 
            return _dbcontext.SaveChanges();
        }

        public IEnumerable<Student> GetAll()
        {
            return _dbcontext.Students.AsNoTracking().ToList();
        }

        public Student GetByID(int id)
        {
            var student = _dbcontext.Students.Find(id);
            return student;
        }

        public int Update(Student student)
        {
            _dbcontext.Students.Update(student); // state added 
            return _dbcontext.SaveChanges();
        }
    }
}
