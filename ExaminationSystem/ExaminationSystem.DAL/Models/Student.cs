using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExaminationSystem.DAL.Models
{
    public class Student
    {
        public int ID { get; set; }

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        [Column(TypeName ="varchar(100)")]
        public string Email { get; set; }


        [Phone]
        [MaxLength(15)]
        public string? Phone { get; set; }

        [MaxLength(200)]
        public string? Address { get; set; }

        public virtual ICollection<Enrollment> Enrollments { get; set; } = new HashSet<Enrollment>();
        public virtual ICollection<Submission> Submissions { get; set; } = new HashSet<Submission>();

    }
}
