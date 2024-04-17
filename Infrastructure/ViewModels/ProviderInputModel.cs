using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
#nullable disable

namespace Infrastructure.ViewModels
{
    public class ProviderInputModel
    {
        [Display(Name = "Title")]
        public string Title { get; set; }

        [Display(Name = "Department")]
        public string DepartmentId { get; set; }
        public IEnumerable<SelectListItem> Departments { get; set; }

        [Display(Name = "Your Color")]
        public string Color { get; set; }

        [Display(Name = "Working Start Time")]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [Display(Name = "Working End Time")]
        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; }

        [Display(Name = "Advisement Types")]
        public string AdvisementTypes { get; set; }

        [Display(Name = "New Student")]
        public bool NewStudent { get; set; }
        [Display(Name = "Current Student")]
        public bool ExistingStudent { get; set; }
        [Display(Name = "Flex Student")]
        public bool FlexStudent { get; set; }
    }
}
