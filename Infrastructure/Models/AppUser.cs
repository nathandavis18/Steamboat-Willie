using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Infrastructure.Models
{
    public class AppUser : IdentityUser
    {
        [Required]
        [DisplayName("First Name")]
        public string? FName { get; set; }
        [Required]
        [DisplayName("Last Name")]
        public string? LName { get; set; }

        [Display(Name = "W Number")]
        public string? WNumber { get; set; }

        [DisplayName("Profile Picture")]
        public string? ProfilePictureURL { get; set; } 

        public bool? GoogleCalendarIntegration { get; set; }

        [NotMapped]
        public string? FullName { get { return FName + " " + LName; } }
    }
}
