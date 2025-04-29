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
    public class StudentRepository : IStudentRepository
    {
        private readonly ExaminationContext _dbcontext;

        public StudentRepository(ExaminationContext context)
        {
            _dbcontext = context;
        }
        public int Add(Student student)
        {
            _dbcontext.Students.Add(student);
            return _dbcontext.SaveChanges();
        }

        public int Delete(Student student)
        {
            _dbcontext.Students.Remove(student);
            return _dbcontext.SaveChanges();
        }

        public IEnumerable<Student> GetAll()
        {
            return _dbcontext.Students.AsNoTracking().ToList();
        }

        public Student GetByID(int id)
        {
            return _dbcontext.Students.Find(id);
        }

        public int Update(Student student)
        {
            _dbcontext.Students.Update(student);
            return _dbcontext.SaveChanges();
        }
    }
}
