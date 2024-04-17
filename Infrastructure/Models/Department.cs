using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Models
{
    public class Department
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Department")]
        [StringLength(4)]
        [Required]
        public string? DepartmentName { get; set; }

        public bool IsDisabled { get; set; }
    }
}
