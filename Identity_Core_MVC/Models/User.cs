using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Identity_Core_MVC.Models
{
    public class User : IdentityUser
    {
        [StringLength(20)]
        [MaxLength(20)]
        [Required]
        public string? FirstName { get; set; }

        [StringLength(20)]
        [MaxLength(20)]
        [Required]
        public string? LastName { get; set; }
    }
}
