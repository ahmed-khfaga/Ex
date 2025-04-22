using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExaminationSystem.DAL.Models
{
    public class Enrollment
    {
        [Key]
        [Column(Order = 0)]
        public int Student_ID { get; set; }

        [Key]
        [Column(Order = 1)]
        public int Course_ID { get; set; }

        public DateTime EnrollmentData { get; set; }

        [ForeignKey("Student_ID")]
        public virtual Student Student { get; set; }

        [ForeignKey("CourseID")]
        public virtual Course Course { get; set; }


    }
}
