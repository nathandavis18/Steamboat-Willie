using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Models
{
    public class Appointment
    {
        [Key]
        public int Id { get; set; }

        public int ProviderAvailabilityId { get; set; }

        public string ClientId { get; set; }

        public int AppointmentTypeId { get; set; }

        public int LocationId { get; set; }

        public string? Description { get; set; }

        public string? StudentComments { get; set; }

        //navigational stuff
        
        [ForeignKey("ProviderAvailabilityId")]
        public ProviderAvailability? ProviderAvailability { get; set; }

        [ForeignKey("ClientId")]
        public Client? Client { get; set; }

        [ForeignKey("AppointmentCategoryId")]
        public AppointmentCategory? AppointmentCategory { get; set; }

        [ForeignKey("LocationId")]
        [Display(Name = "Location")]
        public Location? Location { get; set; }
        
    }
}
        