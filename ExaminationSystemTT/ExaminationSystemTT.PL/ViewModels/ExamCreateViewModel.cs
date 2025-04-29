using ExaminationSystemTT.DAL.Models;
using Microsoft.AspNetCore.Mvc.Rendering; // For SelectList
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ExaminationSystemTT.PL.ViewModels
{
    public class ExamCreateViewModel
    {
        // Properties of the Exam to create
        [Required]
        [Display(Name = "Start Time")]
        public DateTime StartTime { get; set; } = DateTime.Now; // Default value

        [Required]
        [Display(Name = "End Time")]
        public DateTime EndTime { get; set; } = DateTime.Now.AddHours(1); // Default value

        [Required]
        [Display(Name = "Course")]
        public int CourseId { get; set; }

        [Required]
        [Display(Name = "Instructor")]
        public int InstructorId { get; set; }

        // --- Data for Dropdowns ---
        public SelectList? Courses { get; set; }
        public SelectList? Instructors { get; set; }
    }
}
