using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExaminationSystem.DAL.Models
{
    public class Course
    {
        public int ID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }


        [Required]
        public int InstructorID { get; set; }
       
        [Required]
        public DateTime CreationDate { get; set; } 

        [ForeignKey("InstructorID")]
        public virtual Instructor Instructor { get; set; }
       
        public virtual ICollection<Enrollment> Enrollments { get; set; } = new HashSet<Enrollment>();
        public virtual ICollection<Exam> Exams { get; set; } = new HashSet<Exam>();
    }
}
