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
        public string? ProviderAvailabilityId { get; set; }

        public string? ClientId { get; set; }

        public string? Description { get; set; }

        public string? StudentComments { get; set; }

        //navigational stuff
        
        [ForeignKey(nameof(ProviderAvailabilityId))]
        public ProviderAvailability? ProviderAvailability { get; set; }

        [ForeignKey(nameof(ClientId))]
        public Client? Client { get; set; }      
    }
}
        