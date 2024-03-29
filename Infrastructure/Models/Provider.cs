using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Models
{
    public class Provider
    {
        [Key]
        public string? AppUserId { get; set; }

        public int DepartmentId { get; set; }

        public string? Title { get; set; }

        public string? AdvisementTypes {  get; set; } 

        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        [ForeignKey(nameof(AppUserId))]
        public AppUser? AppUser { get; set; }

        [ForeignKey(nameof(DepartmentId))]
        public Department? Department { get; set; }
    }
}
