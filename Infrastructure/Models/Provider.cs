using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Models
{
    public class Provider
    {
        [Key]
        public string AppUserId { get; set; } = default!;

        public int DepartmentId { get; set; } = 1;

        [Required]
        public string? Title { get; set; }

        public string? AdvisementTypes {  get; set; } 

        public string? HexColor { get; set; }

        public TimeSpan StartTime { get; set; } = TimeSpan.Zero;
        public TimeSpan EndTime { get; set; } = TimeSpan.Zero;

        [ForeignKey(nameof(AppUserId))]
        public AppUser? AppUser { get; set; }

        [ForeignKey(nameof(DepartmentId))]
        public Department? Department { get; set; }
    }
}
