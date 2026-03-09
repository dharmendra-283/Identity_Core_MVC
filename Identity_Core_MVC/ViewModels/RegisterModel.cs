using System.ComponentModel.DataAnnotations;

namespace Identity_Core_MVC.ViewModels
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "First Name is Required!")]
        [StringLength(20)]
        [Display(Name = "First Name")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is Required!")]
        [StringLength(20)]
        [Display(Name = "Last Name")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "Email is Required!")]
        [DataType(DataType.EmailAddress)]
        [StringLength(256)]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Password is Required!")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is Required!")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Password does not match.")]
        [Display(Name = "Confirm Password")]
        public string? ConfirmPassword { get; set; }
    }
}
