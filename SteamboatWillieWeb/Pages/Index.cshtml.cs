using DataAccess;
using Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json.Converters;
using System.ComponentModel;
using System.Drawing;
using System.Dynamic;
using Utility;

namespace SteamboatWillieWeb.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly UnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;

        public Calendar[] CalendarObj {  get; set; }
        public List<AppointmentCard> Appointments { get; set; }
        public class Calendar //Test class, needs to be updated
        {
            public string Id { get; set; }
            public string Title { get; set; }
            public string Date { get; set; }
        }

        public class AppointmentCard
        {
            public string Id { get; set; }
            public string ProviderName { get; set; }
            public string AppointmentType { get; set; }
            public string Date { get; set; }
            public string StartTime { get; set; }
            public string EndTime { get; set; }
            public string Location { get; set; }
            public string Color { get; set; }
        }


        public IndexModel(ILogger<IndexModel> logger, UnitOfWork unitOfWork, UserManager<AppUser> userManager)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            Appointments = new List<AppointmentCard>();
        }

        public IActionResult OnGet()
        {
            if (User.IsInRole(SD.CLIENT_ROLE))
            {
                var user = _userManager.GetUserAsync(User).GetAwaiter().GetResult();
                if (user != null)
                {
                    var clientAppointments = _unitOfWork.Appointment.GetAll(includes: "ProviderAvailability").Where(a => a.ClientId == user.Id);
                    foreach(var app in clientAppointments)
                    {
                        Appointments.Add(new AppointmentCard
                        {
                            Id = app.ProviderAvailabilityId,
                            ProviderName = _unitOfWork.AppUser.Get(x => x.Id == app.ProviderAvailability.ProviderId).FullName,
                            AppointmentType = _unitOfWork.Provider.Get(x => x.AppUserId == app.ProviderAvailability.ProviderId).Title,
                            Date = app.ProviderAvailability.StartTime.ToLongDateString(),
                            StartTime = app.ProviderAvailability.StartTime.ToShortTimeString(),
                            EndTime = app.ProviderAvailability.EndTime.ToShortTimeString(),
                            Location = _unitOfWork.Location.Get(x => x.Id == app.ProviderAvailability.LocationId).LocationValue
                        });
                        var x = Appointments.Last();
                        x.Color = GetColor(x.AppointmentType);
                    }
                    Appointments.Add(new AppointmentCard
                    {
                        Id = "3",
                        ProviderName = "Test Tutor",
                        AppointmentType = "Tutor",
                        Date = "Some Day",
                        StartTime = "1:00 PM",
                        EndTime = "2:00 PM",
                        Location = "That Place",
                        Color = GetColor("Tutor")
                    });
                    Appointments.Add(new AppointmentCard
                    {
                        Id = "3",
                        ProviderName = "Test Advisor",
                        AppointmentType = "Advisor",
                        Date = "The Next Day",
                        StartTime = "1:00 PM",
                        EndTime = "2:00 PM",
                        Location = "Other Place",
                        Color = GetColor("Advisor")
                    });
                }
            }
            CalendarObj = new Calendar[]
            {
                new Calendar {Id = "3", Title = "Test", Date = "2024-03-25" }, //Test data, needs to be pulled from DB
                new Calendar {Id = "4", Title = "Test2", Date = "2024-03-26"},
                new Calendar {Id = "5", Title = "Test3", Date = "2024-03-26" }
            };
            return Page();
        }

        string GetColor(string type)
        {
            switch(type)
            {
                case "Instructor":
                    return "#00BFFF";
                case "Tutor":
                    return "#FF6961";
                case "Advisor":
                    return "#00fa9a";
            }
            return "";
        }
    }
}
