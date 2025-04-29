using System.ComponentModel.DataAnnotations;

namespace ExaminationSystemTT.PL.ViewModels
{
    public class SignUpViewModel
    {
        [Required(ErrorMessage ="First Name Is Requried")]
        public string FName { get; set; }


        [Required(ErrorMessage = "Last Name Is Requried")]
        public string LName { get; set; }

        [Required(ErrorMessage ="Email is Required")]
        [EmailAddress]
        public string Email { get; set; }


        [Required(ErrorMessage ="Password IS Requried"  )]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage ="Confirm Password IS Requried")]
        [Compare(nameof(Password),ErrorMessage ="Confirm Password doesn't match Password")]
        [DataType(DataType.Password)]

        public string ConfirmPassword { get; set; }


        [Required (ErrorMessage ="Required To Agree")]
        public bool IsAgree { get; set; }


    }
}
