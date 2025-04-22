using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExaminationSystem.DAL.Models
{
    public class Submission
    {
        public int ID { get; set; }
        [Required]
        public int Student_ID { get; set; } // int in ERD (Matches Student.Id)

        [Required]
        public int Exam_ID { get; set; } 

        [Required]
        public DateTime StartTime { get; set; }
        [Required]
        public DateTime EndTime { get; set; } 

        public int? Score { get; set; } 

        // Navigation Properties
        [ForeignKey("Student_ID")]
        public virtual Student student { get; set; }

        [ForeignKey("Exam_ID")]
        public virtual Exam exam { get; set; }

        public virtual ICollection<Answer> Answers { get; set; } = new HashSet<Answer>();
    }
}
