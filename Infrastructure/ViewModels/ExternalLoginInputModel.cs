using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
#nullable disable

namespace Infrastructure.ViewModels
{
    public class ExternalLoginInputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FName { get; set; }

        [Required]
        [DisplayName("Last Name")]
        public string LName { get; set; }


        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password (Optional)")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Phone]
        [Display(Name = "Phone number")]
        [RegularExpression("\\([0-9]{3}\\) [0-9]{3}-[0-9]{4}", ErrorMessage = "Phone number must follow the format (xxx) xxx-xxxx")]
        [StringLength(15)]
        [Required]
        public string PhoneNumber { get; set; }

        [Display(Name = "W#")]
        [RegularExpression("^W([0-9]{8}$)", ErrorMessage = "W# must match the format of W########")]
        [StringLength(9)]
        public string WNumber { get; set; }
    }
}
