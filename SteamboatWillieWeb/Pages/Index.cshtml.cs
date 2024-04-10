﻿using DataAccess;
using Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Utility;
using static SteamboatWillieWeb.Pages.Appointments.RegisterAppointmentModel;

namespace SteamboatWillieWeb.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly UnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        public string CurrentUserStartTime { get; set; }
        public string CurrentUserEndTime { get; set; }

        [BindProperty]
        public AppointmentModel? AppointmentModelInput {  get; set; }

        public List<AppointmentCard> Appointments { get; set; }
        private IEnumerable<ProviderAvailability> providerAvailabilities;
        public List<Calendar> CalendarObj { get; set; }
        public class Calendar //Test class, needs to be updated
        {
            public string Id { get; set; }
            public string Title { get; set; }
            public string Start { get; set; }
            public string End { get; set; }
            public string Type { get; set; } //appointment or availability, for provider calendar
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
            providerAvailabilities = new List<ProviderAvailability>();
            CalendarObj = new List<Calendar>();
        }

        public IActionResult OnGet()
        {
            if (User.IsInRole(SD.CLIENT_ROLE))
            {
                var user = _userManager.GetUserAsync(User).GetAwaiter().GetResult();
                if (user != null)
                {
                    var clientAppointments = _unitOfWork.Appointment.GetAll(includes: "ProviderAvailability").Where(a => a.ClientId == user.Id).ToList();
                    clientAppointments.Sort((x, y) =>
                        _unitOfWork.ProviderAvailability.Get(p => p.Id == x.ProviderAvailabilityId).StartTime.CompareTo(_unitOfWork.ProviderAvailability.Get(p => p.Id == y.ProviderAvailabilityId).StartTime) //Cool
                    );
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
                    /*Appointments.Add(new AppointmentCard
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
                    });*/
                }
            }
            if(User.IsInRole(SD.PROVIDER_ROLE))
            {
                var currentUserId = _userManager.GetUserId(User);
                providerAvailabilities = _unitOfWork.ProviderAvailability.GetAll().Where(x => x.ProviderId == currentUserId && x.StartTime > DateTime.Now.AddDays(-1));
                var ProviderTitle = _unitOfWork.Provider.GetAll().Where(x => x.AppUserId == currentUserId).Select(x => x.Title).FirstOrDefault();
                string scheduleTitle;
                if (ProviderTitle == "Advisor")
                    scheduleTitle = "Advising";
                else if (ProviderTitle == "Instructor")
                    scheduleTitle = "Office Hours";
                else scheduleTitle = "Tutoring";

                var currentUser = _unitOfWork.Provider.Get(p => p.AppUserId == currentUserId, false, "Department");
                if (currentUser != null)
                {
                    CurrentUserStartTime = currentUser.StartTime?.ToString("HH:mm:ss") ?? "07:00:00"; // Default to 07:00:00 if start time is null
                    CurrentUserEndTime = currentUser.EndTime?.ToString("HH:mm:ss") ?? "19:00:00"; // Default to 19:00:00 if end time is null
                }

                foreach (var availability in providerAvailabilities)
                {
                    if (availability.Scheduled)
                    {
                        var appointment = _unitOfWork.Appointment.GetAll().Where(x => x.ProviderAvailabilityId == availability.Id).FirstOrDefault();
                        var clientName = _unitOfWork.AppUser.GetAll().Where(x => x.Id == appointment.ClientId).Select(x => x.FullName).FirstOrDefault();

                        CalendarObj.Add(new Calendar { Id = availability.Id.ToString(), Title = scheduleTitle + ": " + clientName, Start = DateTimeParser.ParseDateTime(availability.StartTime), End = DateTimeParser.ParseDateTime(availability.EndTime), Type = "Appointment" });
                    }
                    else
                    {
                        CalendarObj.Add(new Calendar { Id = availability.Id.ToString(), Title = scheduleTitle, Start = DateTimeParser.ParseDateTime(availability.StartTime), End = DateTimeParser.ParseDateTime(availability.EndTime), Type = "Availability" });
                    }
                }

            }
            return Page();
        }


        //Cancel Appointment Popup Methods
        public PartialViewResult OnGetAppointmentCancel(string? id)
        {

            var appointment = _unitOfWork.Appointment.Get(a => a.ProviderAvailabilityId == id, includes: "ProviderAvailability");
            var provider = _unitOfWork.ProviderAvailability.Get(pa => pa.Id == id, includes: "Provider").Provider;
            AppointmentModelInput = new AppointmentModel
            {
                AvailabilityId = id,
                Date = appointment.ProviderAvailability.StartTime.ToLongDateString(),
                Time = appointment.ProviderAvailability.StartTime.ToShortTimeString(),
                Location = _unitOfWork.Location.Get(l => l.Id == appointment.ProviderAvailability.LocationId).LocationValue,
                ProviderType = provider.Title,
                ProviderName = _unitOfWork.AppUser.Get(a => a.Id == provider.AppUserId).FullName,
                Comments = appointment.StudentComments,
                IsScheduled = appointment.ProviderAvailability.Scheduled
            };
            return Partial("./Appointments/_CancelAppointmentPartial", this);
        }

        public IActionResult OnPostAppointmentCancel(string? id)
        {
            var appointment = _unitOfWork.Appointment.Get(a => a.ProviderAvailabilityId == id);
            _unitOfWork.Appointment.Delete(appointment);
            var availability = _unitOfWork.ProviderAvailability.Get(pa => pa.Id == id);
            availability.Scheduled = false;
            _unitOfWork.ProviderAvailability.Update(availability);

            _unitOfWork.Commit();

            return RedirectToPage("./Index");
        }

        

        //Details Button Popup Methods
        public PartialViewResult OnGetAppointmentDetails(string? id)
        {
            var appointment = _unitOfWork.Appointment.Get(a => a.ProviderAvailabilityId == id, includes: "ProviderAvailability");
            var provider = _unitOfWork.ProviderAvailability.Get(pa => pa.Id == id, includes: "Provider").Provider;
            AppointmentModelInput = new AppointmentModel
            {
                AvailabilityId = id,
                Date = appointment.ProviderAvailability.StartTime.ToLongDateString(),
                Time = appointment.ProviderAvailability.StartTime.ToShortTimeString(),
                Location = _unitOfWork.Location.Get(l => l.Id == appointment.ProviderAvailability.LocationId).LocationValue,
                ProviderType = provider.Title,
                ProviderName = _unitOfWork.AppUser.Get(a => a.Id == provider.AppUserId).FullName,
                Comments = appointment.StudentComments,
                IsScheduled = appointment.ProviderAvailability.Scheduled
            };
            return Partial("./Appointments/_DetailsAppointmentPartial", this);
        }

        public IActionResult OnPostAppointmentDetails(string? id, string? comments)
        {
            var appointment = _unitOfWork.Appointment.Get(a => a.ProviderAvailabilityId == id);
            appointment.StudentComments = AppointmentModelInput.Comments;

            _unitOfWork.Appointment.Update(appointment);
            _unitOfWork.Commit();

            return RedirectToPage("./Index");
        }

        private string GetColor(string type)
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
