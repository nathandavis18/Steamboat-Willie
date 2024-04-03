using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Models
{
    public class ProviderAvailability
    {
        public ProviderAvailability()
        {
            Id = Guid.NewGuid().ToString(); //Creates an Id for the availability
        }
        [Key]
        public string? Id { get; set; } = default!;

        public string? ProviderId { get; set; }

        [Required]
        [Display(Name = "Start Time")]
        public DateTime StartTime { get; set; }

        [Required]
        [Display(Name = "End Time")]
        public DateTime EndTime { get; set; }

        [Required]
        public DateTime Duration { get; set; }

        public int LocationId { get; set; }

        public bool Scheduled { get; set; }

        [ForeignKey(nameof(ProviderId))]
        public Provider? Provider { get; set; }

        [ForeignKey(nameof(LocationId))]
        [Display(Name = "Location")]
        public Location? Location { get; set; }
    }
}
