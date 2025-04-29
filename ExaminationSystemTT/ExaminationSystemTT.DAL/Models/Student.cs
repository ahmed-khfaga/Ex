using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExaminationSystemTT.DAL.Models
{
    public class Student
    {
        [Key]
        public int StudentId { get; set; }

        [Required(ErrorMessage = "First name is required.")]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(100)]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [StringLength(255)]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        // Consider adding [Index(IsUnique = true)] in your DbContext configuration (Fluent API)
        // if using EF Core for guaranteed database uniqueness.
        public string Email { get; set; }

        [StringLength(20)]
        [Phone(ErrorMessage = "Invalid Phone Number.")]
        public string? Phone { get; set; } // Nullable

        // Navigation property
        public virtual ICollection<StudentAnswer> StudentAnswers { get; set; } = new HashSet<StudentAnswer>();

    }
}
