using DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json.Converters;
using System.ComponentModel;
using System.Dynamic;
using Utility;
using Infrastructure.Models;
using System.Security.Claims;
using Microsoft.CodeAnalysis.Options;

namespace SteamboatWillieWeb.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly UnitOfWork _unitOfWork;
        private IEnumerable<ProviderAvailability> providerAvailabilities;
        public List<Calendar> CalendarObj {  get; set; }
        public class Calendar //Test class, needs to be updated
        {
            public string Id { get; set; }
            public string Title { get; set; }
            public string Start { get; set; }
            public string End { get; set; }
            public string Type {  get; set; } //appointment or availability, for provider calendar
        }

        public IndexModel(ILogger<IndexModel> logger, UnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            providerAvailabilities = new List<ProviderAvailability>();
            CalendarObj = new List<Calendar>();
        }

        public IActionResult OnGet()
        {
            if (User.IsInRole(SD.PROVIDER_ROLE)) {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                providerAvailabilities = _unitOfWork.ProviderAvailability.GetAll().Where(x => x.ProviderId == currentUserId && x.StartTime > DateTime.Now.AddDays(-1));
                var ProviderTitle = _unitOfWork.Provider.GetAll().Where(x => x.AppUserId == currentUserId).Select(x => x.Title).FirstOrDefault();
                string scheduleTitle;
                if (ProviderTitle == "Advisor")
                    scheduleTitle = "Advising";
                else if (ProviderTitle == "Instructor")
                    scheduleTitle = "Office Hours";
                else scheduleTitle = "Tutoring";
                
                foreach (var availability in providerAvailabilities)
                {
                    if (availability.Scheduled)
                    {
                        var appointment = _unitOfWork.Appointment.GetAll().Where(x => x.ProviderAvailabilityId == availability.Id).FirstOrDefault();
                        var clientName = _unitOfWork.AppUser.GetAll().Where(x => x.Id == appointment.ClientId).Select(x => x.FullName).FirstOrDefault();

                        CalendarObj.Add(new Calendar { Id = availability.Id.ToString(), Title = scheduleTitle + ": " + clientName, Start = DateTimeParser.ParseDateTime(availability.StartTime), End = DateTimeParser.ParseDateTime(availability.EndTime), Type = "Appointment"});
                    }
                    else
                    {
                        CalendarObj.Add(new Calendar { Id = availability.Id.ToString(), Title = scheduleTitle, Start = DateTimeParser.ParseDateTime(availability.StartTime), End = DateTimeParser.ParseDateTime(availability.EndTime), Type = "Availability" });
                    }
                }
                //CalendarObj = new Calendar[]
                //{
                //    new Calendar {Id = "3", Title = "Test", Date = "2024-03-25" }, //Test data, needs to be pulled from DB
                //    new Calendar {Id = "4", Title = "Test2", Date = "2024-03-26"},
                //    new Calendar {Id = "5", Title = "Test3", Date = "2024-03-26" }
                //};
                }
            return Page();
        }
    }
}
