using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Models
{
    public class Appointment
    {
        [Key]
        public string? ProviderAvailabilityId { get; set; }

        public string? ClientId { get; set; }

        public string? Description { get; set; }

        public string? StudentComments { get; set; }

        public bool StudentNoShow { get; set; }

        public string? StudentAttachment {  get; set; }

        public string? ProviderAttachment {  get; set; }


        //Google calendar stuff
        public string? ClientEventId { get; set; }
        public string? ProviderEventId {  get; set; }

        
        //navigational stuff
        [ForeignKey(nameof(ProviderAvailabilityId))]
        public ProviderAvailability? ProviderAvailability { get; set; }

        [ForeignKey(nameof(ClientId))]
        public Client? Client { get; set; }      
    }
}
        