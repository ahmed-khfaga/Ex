using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExaminationSystem.DAL.Models
{
    public class Choice
    {
        public int ID { get; set; }

        [Required]
        public int QuestionID { get; set; }

        [Required]
        [MaxLength(255)]
        public string ChoiceText { get; set; } 

        [Required]
        public bool IsCorrect { get; set; } 

        // Navigation Properties
        [ForeignKey("QuestionID")]
        public virtual Question Question { get; set; }

        public virtual ICollection<Answer> SelectedByAnswers { get; set; } = new HashSet<Answer>();
    }
}
