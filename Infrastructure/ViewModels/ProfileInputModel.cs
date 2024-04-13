using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
#nullable disable

namespace Infrastructure.ViewModels
{
    public class ProfileInputModel
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

        [Display(Name = "W#")]
        [RegularExpression("^W([0-9]{8}$)", ErrorMessage = "W# must match the format of W########")]
        [StringLength(9)]
        public string WNumber { get; set; }

        [DisplayName("Profile Picture")]
        public string ProfilePictureURL { get; set; }

        public bool IsIntegrated { get; set; }
    }
}
