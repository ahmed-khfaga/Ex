using System.ComponentModel.DataAnnotations;

namespace ExaminationSystemTT.PL.ViewModels
{
    public class ProfileViewModel
    {
        // Details to display (read-only on this page usually)
        public string FName { get; set; }
        public string LName { get; set; }
        public string Email { get; set; }
        public string? ExistingProfilePicturePath { get; set; } // Current picture path

        // For file upload
        [Display(Name = "New Profile Picture")]
        public IFormFile? ProfilePictureFile { get; set; } // ? makes it optional
    }
}