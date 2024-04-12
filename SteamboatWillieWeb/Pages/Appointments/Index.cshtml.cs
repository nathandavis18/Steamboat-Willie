using DataAccess;
using Google.Apis.Calendar.v3.Data;
using Infrastructure.Models;
using Infrastructure.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.IdentityModel.Tokens;
using Utility;
using Utility.GoogleCalendar;

namespace SteamboatWillieWeb.Pages.Appointments
{
    public class IndexModel : PageModel
    {
        public List<Calendar> CalendarObj { get; set; }
        public class Calendar //Test class, needs to be updated
        {
            public string? Id { get; set; }
            public string? Name { get; set; }
            public string? StartTime { get; set; }
            public string? EndTime {  get; set; }
            public string? Color { get; set; }
        }

        private readonly UnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        private readonly IGoogleCalendarService _googleCalendarService;
        public IndexModel(UnitOfWork unitOfWork, UserManager<AppUser> userManager, IGoogleCalendarService googleCalendarService)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _googleCalendarService = googleCalendarService;
            CalendarObj = new List<Calendar>();
            FilterModelInput = new FilterModel();
        }

        [BindProperty]
        public AppointmentViewModel? InputModel { get; set; }

        [BindProperty]
        public FilterModel? FilterModelInput { get; set; }

        public class FilterModel
        {
            public string? CurrentCampus { get; set; }
            public string? CurrentProvider { get; set; }
            public string? CurrentProviderType { get; set; }
            public string? CurrentAppointmentType { get; set; }
            public string? CurrentClasses { get; set; }

            public List<SelectListItem> Classes { get; set; }
            public List<SelectListItem> Providers { get; set; }
            public List<SelectListItem> AppointmentTypes { get; set; }
            public List<SelectListItem> Campuses { get; set; }
            public List<SelectListItem> ProviderTypes { get; set; }
        }

        public IActionResult OnGet(string? campus, string? providerId, string? providerType, string? appointmentType, string? classes, string? prevProviderType)
        {
            if (!User.Identity!.IsAuthenticated)
            {
                return RedirectToPage("/Account/Login", new { ReturnUrl = "/Appointment/Index", Area = "Identity" });
            }
            var user = _userManager.GetUserAsync(User).GetAwaiter().GetResult();
            if (!User.IsInRole(SD.CLIENT_ROLE))
            {
                TempData["error"] = "Access Denied. If you believe you should have access, report this to the administrator.";
                return RedirectToPage("../Index");
            }
            if(prevProviderType != providerType)
            {
                providerId = String.Empty;
            }

            FilterModelInput.CurrentCampus = campus;
            FilterModelInput.CurrentProvider = providerId;
            FilterModelInput.CurrentClasses = classes;
            FilterModelInput.CurrentProviderType = providerType;
            FilterModelInput.CurrentAppointmentType = appointmentType;


            FilterModelInput.AppointmentTypes = new List<SelectListItem>()
            {
                new SelectListItem { Text = "Office Hours", Value = "Office Hours"},
                new SelectListItem { Text = "General Advisement", Value = "General Advisement"},
                new SelectListItem { Text = "New Student Advisement", Value = "New Student"},
                new SelectListItem { Text = "Flex Student Advisement", Value = "Flex Student"}
            };
            FilterModelInput.Campuses = new List<SelectListItem>()
            {
                new SelectListItem { Text = "Ogden Campus", Value = "Ogden" },
                new SelectListItem {Text = "Davis Campus", Value = "Davis"},
                new SelectListItem { Text = "SLCC Campus", Value = "SLCC"},
                new SelectListItem {Text = "Online", Value = "Online"}
            };
            FilterModelInput.ProviderTypes = new List<SelectListItem>()
            {
                new SelectListItem { Text = "Advisement", Value = "Advisor" },
                new SelectListItem { Text = "Instructing", Value = "Instructor" },
                new SelectListItem { Text = "Tutoring", Value = "Tutor" }
            };

            var allAvailabilities = _unitOfWork.ProviderAvailability.GetAll(includes: "Provider,Location").Where(p => !p.Scheduled && !(p.Provider.AppUserId == user.Id) && p.StartTime >= DateTime.Now);
            var availabilities = allAvailabilities;
            var providers = _unitOfWork.Provider.GetAll(includes: "AppUser").Where(p => allAvailabilities.Select(a => a.ProviderId).Contains(p.AppUserId));
            var allClasses = _unitOfWork.ProviderClass.GetAll(includes: "Class,Provider").Where(pc => availabilities.Select(a => a.ProviderId).Contains(pc.ProviderId)).DistinctBy(pc => pc.Class.Name);
            FilterModelInput.Classes = allClasses.Select(pc => new SelectListItem
                {
                    Text = pc.Class.Id.ToString(),
                    Value = pc.Class.Name
                }).ToList();
            FilterModelInput.Providers = providers.Select(p => new SelectListItem
                {
                    Text = p.AppUser.FullName,
                    Value = p.AppUserId
                }).ToList();
            
            if (!String.IsNullOrEmpty(campus))
            {
                availabilities = availabilities.Where(a => a.Location.Campus == campus);
            }
            if (!String.IsNullOrEmpty(providerId))
            {
                availabilities = availabilities.Where(a => a.ProviderId == providerId);
            }
            if (!String.IsNullOrEmpty(providerType))
            {
                availabilities = availabilities.Where(a => a.Provider.Title == providerType);
                providers = providers.Where(p => p.Title == providerType);
                FilterModelInput.Providers = providers.Select(p => new SelectListItem
                    {
                        Text = p.AppUser.FullName,
                        Value = p.AppUserId
                    }).ToList();
            }
            if (!String.IsNullOrEmpty(appointmentType))
            {
                availabilities = availabilities.Where(a => a.AppointmentType == appointmentType);
            }
            if (!String.IsNullOrEmpty(classes))
            {
                var providerClasses = _unitOfWork.ProviderClass.GetAll(includes: "Provider,Class").Where(pc => classes.Contains(pc.Class.Name));
                availabilities = availabilities.Where(a => providerClasses.Select(pc => pc.ProviderId).Contains(a.ProviderId));
            }
            foreach (var a in availabilities)
            {
                    CalendarObj.Add(new Calendar
                    {
                        Id = a.Id,
                        Name = _unitOfWork.AppUser.GetById(a.ProviderId).FullName,
                        StartTime = DateTimeParser.ParseDateTime(a.StartTime),
                        EndTime = DateTimeParser.ParseDateTime(a.EndTime),
                        Color = a.Provider.HexColor
                    });
              }
            return Page();
        }

        //On Event Click
        public async Task<IActionResult> OnGetRegisterAppointmentAsync(string? id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (!await _userManager.IsInRoleAsync(user, SD.CLIENT_ROLE))
            {
                TempData["error"] = "You don't have access to that page";
                return RedirectToPage("./Index");
            }

            var availability = _unitOfWork.ProviderAvailability.GetById(id);
            if (availability == null)
            {
                TempData["error"] = "You don't have access to that page";
                return RedirectToPage("./Index");
            }
            var provider = _unitOfWork.ProviderAvailability.Get(pa => pa.Id == id, includes: "Provider").Provider;
            InputModel = new AppointmentViewModel
            {
                AvailabilityId = id,
                Date = availability.StartTime.ToLongDateString(),
                Time = availability.StartTime.ToShortTimeString(),
                Location = _unitOfWork.Location.GetById(availability.LocationId).LocationValue,
                ProviderType = provider.Title,
                ProviderName = _unitOfWork.AppUser.GetById(provider.AppUserId).FullName
            };
            return Partial("./_RegisterAppointmentPartial", this);
        }

        public async Task<IActionResult> OnPostRegisterAppointmentAsync(string id)
        {
            var clientId = _userManager.GetUserId(User);
            var availability = _unitOfWork.ProviderAvailability.GetById(id);
            var providerId = availability.ProviderId;
            availability.Scheduled = true;
            _unitOfWork.ProviderAvailability.Update(availability);

            Appointment appointment = new Appointment
            {
                ProviderAvailabilityId = id,
                ClientId = clientId,
                StudentComments = InputModel.Comments,
                Description = "Appointment",
                StudentNoShow = false,
                ClientEventId = null,
                ProviderEventId = null
            };

            var location = _unitOfWork.Location.GetById(availability.LocationId).LocationValue;

            if (_unitOfWork.AppUser.GetById(clientId).GoogleCalendarIntegration.Value)
            {
                string summary = availability.AppointmentType + " with " + _unitOfWork.AppUser.GetById(providerId).FullName;
                Event @event = EventCreater.MakeEvent(summary, location, appointment.Description, availability.StartTime.ToString(), availability.EndTime.ToString());
                string clientCalendarId = await _googleCalendarService.CreateEvent(@event, clientId, new CancellationToken(false));
                appointment.ClientEventId = clientCalendarId;
            }

            if (_unitOfWork.AppUser.GetById(providerId).GoogleCalendarIntegration.Value)
            {
                string summary = availability.AppointmentType + " with " + _unitOfWork.AppUser.GetById(clientId).FullName;
                Event @event = EventCreater.MakeEvent(summary, location, appointment.Description, availability.StartTime.ToString(), availability.EndTime.ToString());
                string providerCalendarId = await _googleCalendarService.CreateEvent(@event, providerId, new CancellationToken(false));
                appointment.ProviderEventId = providerCalendarId;
            }

            _unitOfWork.Appointment.Add(appointment);
            await _unitOfWork.CommitAsync();


            return RedirectToPage("../Index");
        }
    }
}
