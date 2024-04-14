using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.Rendering;
#nullable disable

namespace Infrastructure.ViewModels
{
    public class RegisterInputModel
    {
        [Required]
        [Display(Name = "First Name")]
        public string FName { get; set; }

        [Required]
        [DisplayName("Last Name")]
        public string LName { get; set; }

        [Phone]
        [Display(Name = "Phone number")]
        [RegularExpression("\\([0-9]{3}\\) [0-9]{3}-[0-9]{4}", ErrorMessage = "Phone number must follow the format (xxx) xxx-xxxx")]
        [StringLength(15)]
        [Required]
        public string PhoneNumber { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "W#")]
        [RegularExpression("^W([0-9]{8}$)", ErrorMessage = "W# must match the format of W########")]
        [StringLength(9)]
        public string WNumber { get; set; }

        [Display(Name = "Role")]
        public string Role { get; set; }
        public IEnumerable<SelectListItem> RoleList { get; set; }

        public bool CreatingProvider { get; set; }
    }
}
