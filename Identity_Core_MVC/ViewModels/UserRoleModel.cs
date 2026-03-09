using Identity_Core_MVC.Models;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Identity_Core_MVC.ViewModels
{
    public class UserRoleModel
    {
        public string? UserId { get; set; }
        public string? RoleId { get; set; }
        public string? UserName { get; set; }

        public bool IsSelected { get; set; }
    }
}
