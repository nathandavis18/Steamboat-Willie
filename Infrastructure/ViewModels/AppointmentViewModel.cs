using System.ComponentModel.DataAnnotations;

namespace Infrastructure.ViewModels
{
    public class AppointmentViewModel
    {
        public string AvailabilityId { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string Location { get; set; }
        public string ProviderType { get; set; }
        public string ProviderName { get; set; }
        [Display(Name = "Comments")]
        public string Comments { get; set; }

        public bool IsScheduled { get; set; }
    }
}
