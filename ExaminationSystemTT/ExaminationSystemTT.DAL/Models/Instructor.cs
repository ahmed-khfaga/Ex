using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExaminationSystemTT.DAL.Models
{
    public class Instructor
    {
        [Key]
        public int InstructorId { get; set; }

        [Required(ErrorMessage = "First name is required.")]
        [StringLength(100)]
        public string FirstName { get; set; }
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";


        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(100)]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [StringLength(255)]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        // Consider adding uniqueness constraint in DbContext configuration
        public string Email { get; set; }

        [StringLength(20)]
        [Phone(ErrorMessage = "Invalid Phone Number.")]
        public string? Phone { get; set; } // Nullable

        // Navigation property
        public virtual ICollection<Exam> Exams { get; set; } = new HashSet<Exam>();
    }
}