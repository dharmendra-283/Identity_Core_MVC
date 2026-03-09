using Identity_Core_MVC.Models;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Identity_Core_MVC.ViewModels
{
    public class RoleModel
    {
        public string? Id { get; set; }

        [Required(ErrorMessage = "RoleName is Required!")]
        [Display(Name = "Role Name")]
        public string RoleName { get; set; }

        public List<User> Users { get; set; } = new List<User>();
    }
}
