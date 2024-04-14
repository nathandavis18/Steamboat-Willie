using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
#nullable disable

namespace Infrastructure.ViewModels
{
    public class WeberStudentInputModel
    {
        //Only Current Weber Students fill this stuff out
        [Display(Name = "Major")]
        public string DepartmentId { get; set; }
        public IEnumerable<SelectListItem> Departments { get; set; }

        [Display(Name = "Class Level")]
        public string ClassLevel { get; set; }

        [Display(Name = "Student Type")]
        public string StudentType { get; set; }

        [Required]
        [Display(Name = "Weber Student")]
        public bool IsWeberStudent { get; set; }
    }
}
