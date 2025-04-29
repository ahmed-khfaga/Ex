using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExaminationSystemTT.DAL.Models
{
    public class Course
    {
        [Key]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Course name is required.")]
        [StringLength(150)]
        public string Name { get; set; }

        // Navigation property
        public virtual ICollection<Exam> Exams { get; set; } = new HashSet<Exam>();
    }
}