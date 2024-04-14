using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Models
{
    public class Client
    {
        [Key]
        public string? AppUserId { get; set; }

        public int? DepartmentId { get; set; }

        public string? ClassLevel { get; set; }

        [Display(Name = "Student Type")]
        public string? StudentType { get; set; }

        public bool? IsWeberStudent { get; set; }

        [ForeignKey(nameof(AppUserId))]
        public AppUser? AppUser { get; set; }

        [ForeignKey(nameof(DepartmentId))]
        public Department? Department { get; set; }

    }
}
