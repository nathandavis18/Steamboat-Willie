using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Models
{
    public class Department
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Department")]
        public string? DepartmentName { get; set; }

        public bool? IsDisabled { get; set; }
    }
}
