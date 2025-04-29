using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExaminationSystemTT.DAL.Models
{
    public class ExamAttempt
    {
        [Key]
        public int ExamAttemptId { get; set; }

        [Required]
        public int StudentId { get; set; }
        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        [Required]
        public int ExamId { get; set; }
        [ForeignKey("ExamId")]
        public virtual Exam Exam { get; set; }

        public DateTime StartTime { get; set; } // When the student first loaded the exam
        public DateTime? SubmissionTime { get; set; } // When the student submitted (nullable until submitted)
        public int? Score { get; set; } // Nullable until submitted/graded
        public int? MaxScore { get; set; } // Nullable until submitted/graded

        public bool IsCompleted { get; set; } = false; // Flag to mark completion
    }
}
