using System.ComponentModel.DataAnnotations;

namespace Identity_Core_MVC.ViewModels
{
    public class LoginModel
    {
        [Required(ErrorMessage = "UserName is Required!")]
        [Display(Name = "User Name")]
        public string? UserName { get; set; }

        [Required(ErrorMessage = "Password is Required!")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }
    }
}
