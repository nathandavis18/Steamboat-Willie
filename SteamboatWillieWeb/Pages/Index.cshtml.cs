using DataAccess;
using Google.Apis.Auth.OAuth2;
using Infrastructure.Models;
using Infrastructure.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Utility;
using Utility.GoogleCalendar;

namespace SteamboatWillieWeb.Pages
{
    public class IndexModel : PageModel
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        private readonly IGoogleCalendarService _googleCalendarService;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;
        public string CurrentUserStartTime { get; set; }
        public string CurrentUserEndTime { get; set; }

        [BindProperty]
        public AppointmentViewModel? AppointmentModelInput {  get; set; }

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

        [BindProperty]
        public AvailabilityModel? AvailabilityModelInput { get; set; }

        public bool IntegratePopup {  get; set; }

        public IndexModel(UnitOfWork unitOfWork, UserManager<AppUser> userManager, IEmailSender emailSender, IGoogleCalendarService googleCalendarService, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _emailSender = emailSender;
            _googleCalendarService = googleCalendarService;
            Appointments = new List<AppointmentCard>();
            providerAvailabilities = new List<ProviderAvailability>();
            CalendarObj = new List<Calendar>();
            _configuration = configuration;
        }

        public async Task<IActionResult> OnGetAsync(string integratePopup = "")
        {
            var user = await _userManager.GetUserAsync(User);
            if(user != null)
            {
                if (integratePopup.Equals("Integrate"))
                {
                    return Partial("_GoogleCalendarPartial", this);
                }
                if(user.GoogleCalendarIntegration == null)
                {
                    IntegratePopup = true;
                }
            }

            if (User.IsInRole(SD.CLIENT_ROLE))
            {
                if (user != null)
                {
                    var clientAppointments = _unitOfWork.Appointment.GetAll(includes: "ProviderAvailability").Where(a => a.ClientId == user.Id).ToList();
                    clientAppointments.Sort((x, y) =>
                        _unitOfWork.ProviderAvailability.GetById(x.ProviderAvailabilityId).StartTime.CompareTo(_unitOfWork.ProviderAvailability.GetById(y.ProviderAvailabilityId).StartTime) //Cool
                    );
                    foreach(var app in clientAppointments)
                    {
                        Appointments.Add(new AppointmentCard
                        {
                            Id = app.ProviderAvailabilityId,
                            ProviderName = _unitOfWork.AppUser.GetById(app.ProviderAvailability.ProviderId).FullName,
                            AppointmentType = _unitOfWork.Provider.GetById(app.ProviderAvailability.ProviderId).Title,
                            Date = app.ProviderAvailability.StartTime.ToLongDateString(),
                            StartTime = app.ProviderAvailability.StartTime.ToShortTimeString(),
                            EndTime = app.ProviderAvailability.EndTime.ToShortTimeString(),
                            Location = _unitOfWork.Location.GetById(app.ProviderAvailability.LocationId).LocationValue
                        });
                        var x = Appointments.Last();
                        x.Color = GetColor(x.AppointmentType);
                    }
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

        //Google Calendar Integration Question
        public async Task<IActionResult> OnPostGoogleCalendarIntegrationAsync(string integrate)
        {
            var user = await _userManager.GetUserAsync(User);
            user.GoogleCalendarIntegration = integrate.Equals("absolutely");

            if (integrate.Equals("absolutely"))
            {
                var settings = _configuration.GetSection("Authentication:Google");
                string[] scope = new string[] { "https://www.googleapis.com/auth/calendar" };
                UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets()
                {
                    ClientId = settings["ClientId"],
                    ClientSecret = settings["ClientSecret"],
                },
                scope,
                user.Id,
                new CancellationToken(false));
            }

            _unitOfWork.AppUser.Update(user);
            await _unitOfWork.CommitAsync();

            return RedirectToPage("./Index");
        }


        //Cancel Appointment Popup Methods
        public async Task<IActionResult> OnGetAppointmentCancelAsync(string? id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (!await _userManager.IsInRoleAsync(user, SD.CLIENT_ROLE))
            {
                TempData["error"] = "You don't have access to that page";
                return RedirectToPage("./Index");
            }

            var appointment = _unitOfWork.Appointment.Get(a => a.ProviderAvailabilityId == id, includes: "ProviderAvailability");
            if (appointment == null)
            {
                TempData["error"] = "An error occured, please try again";
                return RedirectToPage("./Index");
            }

            var provider = _unitOfWork.ProviderAvailability.Get(pa => pa.Id == id, includes: "Provider").Provider;
            AppointmentModelInput = new AppointmentViewModel
            {
                AvailabilityId = id,
                Date = appointment.ProviderAvailability.StartTime.ToLongDateString(),
                Time = appointment.ProviderAvailability.StartTime.ToShortTimeString(),
                Location = _unitOfWork.Location.GetById(appointment.ProviderAvailability.LocationId).LocationValue,
                ProviderType = provider.Title,
                ProviderName = _unitOfWork.AppUser.GetById(provider.AppUserId).FullName,
                Comments = appointment.StudentComments,
                IsScheduled = appointment.ProviderAvailability.Scheduled
            };
            return Partial("./Appointments/_CancelAppointmentPartial", this);
        }

        public async Task<IActionResult> OnPostAppointmentCancelAsync(string? id)
        {
            var appointment = _unitOfWork.Appointment.GetById(id);
            string clientCalendarId = appointment.ClientEventId;
            string clientId = appointment.ClientId!;
            string providerId = _unitOfWork.ProviderAvailability.GetById(appointment.ProviderAvailabilityId).ProviderId;
            string providerCalendarId = appointment.ProviderEventId;

            _unitOfWork.Appointment.Delete(appointment);

            var availability = _unitOfWork.ProviderAvailability.GetById(id);
            availability.Scheduled = false;
            _unitOfWork.ProviderAvailability.Update(availability);

            await _unitOfWork.CommitAsync();

            if (!String.IsNullOrEmpty(clientCalendarId) && _unitOfWork.AppUser.GetById(clientId).GoogleCalendarIntegration.Value)
            {
                await _googleCalendarService.DeleteEvent(clientCalendarId, clientId, new CancellationToken(false));
            }

            if (!String.IsNullOrEmpty(providerCalendarId) && _unitOfWork.AppUser.GetById(providerId).GoogleCalendarIntegration.Value)
            {
                await _googleCalendarService.DeleteEvent(providerCalendarId, providerId, new CancellationToken(false));
            }

            return RedirectToPage("./Index");
        }

        

        //Details Button Popup Methods
        public async Task<IActionResult> OnGetAppointmentDetailsAsync(string? id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (!await _userManager.IsInRoleAsync(user, SD.CLIENT_ROLE))
            {
                TempData["error"] = "You don't have access to that page";
                return RedirectToPage("./Index");
            }

            var appointment = _unitOfWork.Appointment.Get(a => a.ProviderAvailabilityId == id, includes: "ProviderAvailability");

            if (appointment == null)
            {
                TempData["error"] = "An error occured, please try again";
                return RedirectToPage("./Index");
            }

            var provider = _unitOfWork.ProviderAvailability.Get(pa => pa.Id == id, includes: "Provider").Provider;
            AppointmentModelInput = new AppointmentViewModel
            {
                AvailabilityId = id,
                Date = appointment.ProviderAvailability.StartTime.ToLongDateString(),
                Time = appointment.ProviderAvailability.StartTime.ToShortTimeString(),
                Location = _unitOfWork.Location.GetById(appointment.ProviderAvailability.LocationId).LocationValue,
                ProviderType = provider.Title,
                ProviderName = _unitOfWork.AppUser.GetById(provider.AppUserId).FullName,
                Comments = appointment.StudentComments,
                IsScheduled = appointment.ProviderAvailability.Scheduled
            };
            return Partial("./Appointments/_DetailsAppointmentPartial", this);
        }

        public IActionResult OnPostAppointmentDetails(string? id, string? comments)
        {
            var appointment = _unitOfWork.Appointment.GetById(id);
            appointment.StudentComments = AppointmentModelInput.Comments;

            _unitOfWork.Appointment.Update(appointment);
            _unitOfWork.Commit();

            return RedirectToPage("./Index");
        }

        //Provider Availability Popup Stuff
        public async Task<IActionResult> OnGetViewAvailabilityAsync(string? id)
        {
            var user = await _userManager.GetUserAsync(User);
            if(!await _userManager.IsInRoleAsync(user, SD.PROVIDER_ROLE))
            {
                TempData["error"] = "You don't have access to that page";
                return RedirectToPage("./Index");
            }
            var appointment = _unitOfWork.Appointment.GetById(id);
            var availability = _unitOfWork.ProviderAvailability.Get(pa => pa.Id == id, includes: "Location");
            if(availability == null)
            {
                TempData["error"] = "An error occured, please try again";
                return RedirectToPage("./Index");
            }

            AvailabilityModelInput = new AvailabilityModel
            {
                AvailabilityId = id,
                Location = availability.Location.LocationValue,
                Date = availability.StartTime.ToLongDateString(),
                Time = availability.StartTime.ToShortTimeString(),
            };
            if (appointment != null)
            {
                var client = _unitOfWork.Client.Get(c => c.AppUserId == appointment.ClientId, includes: "AppUser");
                AvailabilityModelInput.WNumber = client.AppUser.WNumber;
                AvailabilityModelInput.Comments = appointment.StudentComments;
                AvailabilityModelInput.ClientName = client.AppUser.FullName;
                AvailabilityModelInput.ClientEmail = client.AppUser.Email;
                AvailabilityModelInput.IsAppointment = true;
            }
            return Partial("./Availability/_ViewAvailabilityPartial", this);
        }

        public IActionResult OnPostRemoveAvailability(string id)
        {
            var availability = _unitOfWork.ProviderAvailability.GetById(id);
            _unitOfWork.ProviderAvailability.Delete(availability);
            _unitOfWork.Commit();

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnPostRemoveAppointmentAsync(string id)
        {
            var availability = _unitOfWork.ProviderAvailability.GetById(id);
            var appointment = _unitOfWork.Appointment.GetById(id);

            var clientCalendarId = appointment.ClientEventId;
            var providerCalendarId = appointment.ProviderEventId;
            var clientId = appointment.ClientId;
            var providerId = availability.ProviderId;

            var reason = !String.IsNullOrEmpty(AvailabilityModelInput.CancelReason) ? AvailabilityModelInput.CancelReason : "None";

            var clientEmail = _unitOfWork.AppUser.GetById(appointment.ClientId).Email;
            var providerName = _unitOfWork.AppUser.GetById(availability.ProviderId).FullName;

            var callbackLink = Url.Page("/Appointments/Index", pageHandler: null, values: null, protocol: Request.Scheme);
            await _emailSender.SendEmailAsync(clientEmail, "Appointment Canceled", EmailFormats.AppointmentCanceled.Replace("[ProviderName]", providerName)
                                                                                               .Replace("[ProviderReason]", reason)
                                                                                               .Replace("[Link]", callbackLink));

            _unitOfWork.Appointment.Delete(appointment);

            availability.Scheduled = false;
            _unitOfWork.ProviderAvailability.Update(availability);

            await _unitOfWork.CommitAsync();

            if (!String.IsNullOrEmpty(clientCalendarId) && _unitOfWork.AppUser.GetById(clientId).GoogleCalendarIntegration.Value)
            {
                await _googleCalendarService.DeleteEvent(clientCalendarId, clientId, new CancellationToken(false));
            }

            if (!String.IsNullOrEmpty(providerCalendarId) && _unitOfWork.AppUser.GetById(providerId).GoogleCalendarIntegration.Value)
            {
                await _googleCalendarService.DeleteEvent(providerCalendarId, providerId, new CancellationToken(false));
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnPostRemoveAllAsync(string id)
        {
            var appointment = _unitOfWork.Appointment.GetById(id);
            var availability = _unitOfWork.ProviderAvailability.GetById(id);
            var reason = !String.IsNullOrEmpty(AvailabilityModelInput.CancelReason) ? AvailabilityModelInput.CancelReason : "None";

            var clientCalendarId = appointment.ClientEventId;
            var providerCalendarId = appointment.ProviderEventId;
            var clientId = appointment.ClientId;
            var providerId = availability.ProviderId;

            var clientEmail = _unitOfWork.AppUser.GetById(appointment.ClientId).Email;
            var providerName = _unitOfWork.AppUser.GetById(availability.ProviderId).FullName;

            var callbackLink = Url.Page("/Appointments/Index", pageHandler: null, values: null, protocol: Request.Scheme);
            await _emailSender.SendEmailAsync(clientEmail, "Appointment Canceled", EmailFormats.AppointmentCanceled.Replace("[ProviderName]", providerName)
                                                                                               .Replace("[ProviderReason]", reason)
                                                                                               .Replace("[Link]", callbackLink));

            _unitOfWork.Appointment.Delete(appointment);
            _unitOfWork.ProviderAvailability.Delete(availability);
            await _unitOfWork.CommitAsync();

            if (!String.IsNullOrEmpty(clientCalendarId) && _unitOfWork.AppUser.GetById(clientId).GoogleCalendarIntegration.Value)
            {
                await _googleCalendarService.DeleteEvent(clientCalendarId, clientId, new CancellationToken(false));
            }

            if (!String.IsNullOrEmpty(providerCalendarId) && _unitOfWork.AppUser.GetById(providerId).GoogleCalendarIntegration.Value)
            {
                await _googleCalendarService.DeleteEvent(providerCalendarId, providerId, new CancellationToken(false));
            }

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
