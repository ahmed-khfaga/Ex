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
    public class InsrtuctorRepository : IInstructorRepository
    {

        private readonly ExaminationContext _dbcontext;
        public InsrtuctorRepository(ExaminationContext dbcontext)
        {
            _dbcontext = dbcontext;
        }
        public int Add(Instructor instructor)
        {
            _dbcontext.Instructors.Add(instructor); // state added 
            return _dbcontext.SaveChanges();
        }

        public int Delete(Instructor instructor)
        {
            _dbcontext.Instructors.Remove(instructor); // state added 
            return _dbcontext.SaveChanges();
        }

        public IEnumerable<Instructor> GetAll()
        {
            return _dbcontext.Instructors.AsNoTracking().ToList();
        }

        public Instructor GetByID(int id)
        {
            var instructor = _dbcontext.Instructors.Find(id);
            return instructor;
        }

        public int Update(Instructor instructor)
        {
            _dbcontext.Instructors.Update(instructor); // state added 
            return _dbcontext.SaveChanges();
        }
    }
}
