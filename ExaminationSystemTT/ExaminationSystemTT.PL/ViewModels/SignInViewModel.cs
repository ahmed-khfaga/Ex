using System.ComponentModel.DataAnnotations;

namespace ExaminationSystemTT.PL.ViewModels
{
    public class SignInViewModel
    {

        [Required(ErrorMessage ="Email is Required")]
        [EmailAddress(ErrorMessage ="InValid Email")]
        public string Email { get; set; }

        [Required(ErrorMessage ="Password Is Required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
