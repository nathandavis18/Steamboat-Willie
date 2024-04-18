using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
#nullable disable

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
        public string Campus { get; set; }
        public string studentAttachment { get; set; }
        public string providerAttachment { get; set; }
        public IFormFile studentFile { get; set; }
        public IFormFile providerFile { get; set; }
    }
}
