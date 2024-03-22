using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Models
{
    public class Client
    {
        [Key]
        public string? AppUserId { get; set; }

        [Required]
        public int DepartmentId { get; set; }

        [Required]
        public string? ClassLevel { get; set; }

        [Required]
        [Display(Name = "Student Type")]
        public string? StudentType { get; set; }

        [ForeignKey("AppUserId")]
        public AppUser? AppUser { get; set; }

        [ForeignKey("DepartmentId")]
        public Department? Department { get; set; }

    }
}
