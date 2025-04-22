using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExaminationSystem.DAL.Models
{
    public class Question
    {
        public int ID { get; set; }

        [Required]
        public int ExamID { get; set; }

        [Required]
        public string QuestionText { get; set; } 

        [Required]
        [MaxLength(10)]
        public string QuestionType { get; set; } //  ( "MCQ", "True and False")

        [Required]
        public int Mark { get; set; } 

        public bool? CorrectTFAnswer { get; set; }

        [ForeignKey("ExamID")]
        public virtual Exam Exam { get; set; }

        public virtual ICollection<Choice> Choices { get; set; } = new HashSet<Choice>();
        public virtual ICollection<Answer> Answers { get; set; } = new HashSet<Answer>();
    }
}
