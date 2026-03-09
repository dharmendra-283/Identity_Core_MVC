using System.ComponentModel.DataAnnotations;

namespace Identity_Core_MVC.ViewModels
{
    public class ChangePasswordModel
    {
        [Required(ErrorMessage = "Current Password is Required!")]
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string? CurrentPassword { get; set; }

        [Required(ErrorMessage = "New Password is Required!")]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string? NewPassword { get; set; }

        [Required(ErrorMessage = "Confirm New Password is Required!")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "New Password does not match.")]
        [Display(Name = "Confirm New Password")]
        public string? ConfirmNewPassword { get; set; }
    }
}
