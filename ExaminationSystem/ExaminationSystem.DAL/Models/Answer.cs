using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExaminationSystem.DAL.Models
{
    public class Answer
    {
        public int ID { get; set; }

        [Required]
        public int Subm_ID { get; set; } 

        [Required]
        public int QuestionID { get; set; } 

        public int? SelectedChoiceID { get; set; }

        public bool? SelectedTFAnswer { get; set; }

        public bool? IsCorrect { get; set; }


        [ForeignKey("Subm_ID")]
        public virtual Submission submission { get; set; }

        [ForeignKey("QuestionID")]
        public virtual Question question { get; set; }

        [ForeignKey("SelectedChoiceID")]
        public virtual Choice? selectedChoice { get; set; }
    }
}
