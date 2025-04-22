using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExaminationSystem.DAL.Models
{
    public class Exam
    {
        public int ID { get; set; }


        [Required]
        [MaxLength(150)]
        public string Title { get; set; }

        [Required]
        public int Duration { get; set; }
        
        [Required]
        // Foreign Key to Course.Id (bigint)
        public int CourseID { get; set; }
      
        [Required]
        public int InstructorID { get; set; }
        [ForeignKey("CourseID")]
        public virtual Course Course { get; set; }

        [ForeignKey("InstructorID")]
        public virtual Instructor Instructor { get; set; } // Renamed Nav Prop from ExamsCreated

        public virtual ICollection<Question> Questions { get; set; } = new HashSet<Question>();
        public virtual ICollection<Submission> Submissions { get; set; } = new HashSet<Submission>();
    }
}
