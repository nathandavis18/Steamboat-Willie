using DataAccess;
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
        public string? CurrentUserStartTime { get; set; }
        public string? CurrentUserEndTime { get; set; }

        [BindProperty]
        public AppointmentViewModel? AppointmentModelInput {  get; set; }

        public List<AppointmentCalendarModel> Appointments { get; set; }
        private IEnumerable<ProviderAvailability> providerAvailabilities;
        public List<AppointmentCalendarModel> CalendarObj { get; set; }
        public IWebHostEnvironment WebHostEnvironment { get; set; }

        [BindProperty]
        public AvailabilityModel? AvailabilityModelInput { get; set; }

        public bool IntegrateModal {  get; set; }
        public string? PartialViewName {  get; set; }
        public string? CurrentView {  get; set; }

        public IndexModel(UnitOfWork unitOfWork, UserManager<AppUser> userManager, IEmailSender emailSender, IGoogleCalendarService googleCalendarService, IConfiguration configuration, IWebHostEnvironment web)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _emailSender = emailSender;
            _googleCalendarService = googleCalendarService;
            Appointments = new List<AppointmentCalendarModel>();
            providerAvailabilities = new List<ProviderAvailability>();
            CalendarObj = new List<AppointmentCalendarModel>();
            _configuration = configuration;
            WebHostEnvironment = web;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if(user != null)
            {
                if(user.GoogleCalendarIntegration == null)
                {
                    IntegrateModal = true;
                    PartialViewName = "_GoogleCalendarPartial";
                    return Page();
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
                        Appointments.Add(new AppointmentCalendarModel
                        {
                            Id = app.ProviderAvailabilityId,
                            ProviderName = _unitOfWork.AppUser.GetById(app.ProviderAvailability.ProviderId).FullName,
                            AppointmentType = _unitOfWork.Provider.GetById(app.ProviderAvailability.ProviderId).Title,
                            Date = app.ProviderAvailability.StartTime.ToLongDateString(),
                            StartTime = app.ProviderAvailability.StartTime.ToShortTimeString(),
                            EndTime = app.ProviderAvailability.EndTime.ToShortTimeString(),
                            Location = _unitOfWork.Location.GetById(app.ProviderAvailability.LocationId).LocationValue,
                            Campus = _unitOfWork.Location.GetById(app.ProviderAvailability.LocationId).Campus
                        });
                        var x = Appointments.Last();
                        x.Color = GetColor(x.AppointmentType);
                    }
                    CurrentView = "Client";
                }
            }
            if(User.IsInRole(SD.PROVIDER_ROLE))
            {
                var currentUserId = _userManager.GetUserId(User);
                providerAvailabilities = _unitOfWork.ProviderAvailability.GetAll().Where(x => x.ProviderId == currentUserId/* && x.StartTime > DateTime.Now.AddDays(-1)*/);
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
                    CurrentUserStartTime = currentUser.StartTime.ToString() ?? "07:00:00"; // Default to 07:00:00 if start time is null
                    CurrentUserEndTime = currentUser.EndTime.ToString() ?? "19:00:00"; // Default to 19:00:00 if end time is null
                }

                foreach (var availability in providerAvailabilities)
                {
                    if (availability.Scheduled)
                    {
                        var appointment = _unitOfWork.Appointment.GetAll().Where(x => x.ProviderAvailabilityId == availability.Id).FirstOrDefault();
                        var clientName = _unitOfWork.AppUser.GetAll().Where(x => x.Id == appointment.ClientId).Select(x => x.FullName).FirstOrDefault();

                        CalendarObj.Add(new AppointmentCalendarModel { Id = availability.Id.ToString(), ProviderType = scheduleTitle + ": " + clientName, StartTime = DateTimeParser.ParseDateTime(availability.StartTime), EndTime = DateTimeParser.ParseDateTime(availability.EndTime), Type = "Appointment" });
                    }
                    else
                    {
                        CalendarObj.Add(new AppointmentCalendarModel { Id = availability.Id.ToString(), ProviderType = scheduleTitle, StartTime = DateTimeParser.ParseDateTime(availability.StartTime), EndTime = DateTimeParser.ParseDateTime(availability.EndTime), Type = "Availability" });
                    }
                }
                CurrentView = "Provider";
            }
            return Page();
        }

        //Google Calendar Integration Question
        public async Task<IActionResult> OnPostGoogleCalendarIntegrationAsync(bool integrate)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { ReturnUrl = "/Index", Area = "Identity" });
            }
            if (integrate)
            {
                var credential = await ValidateUser.ValidateUserCalendar(user!.Id, _configuration);
                user.GoogleCalendarIntegration = ValidateUser.IsUserValidated(credential);
            }
            else
            {
                user.GoogleCalendarIntegration = false;
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
                IsScheduled = appointment.ProviderAvailability.Scheduled,
                Campus = _unitOfWork.Location.GetById(appointment.ProviderAvailability.LocationId).Campus,
                studentAttachment = appointment.StudentAttachment,
                providerAttachment = appointment.ProviderAttachment,
                studentFile = null,
                providerFile = null
            };
            return Partial("./Appointments/_DetailsAppointmentPartial", this);
        }

        public IActionResult OnPostAppointmentDetails(string? id, string? comments)
        {
            var appointment = _unitOfWork.Appointment.GetById(id);
            if (comments != null)
            {
                appointment.StudentComments = AppointmentModelInput.Comments;
            }
            if (AppointmentModelInput.studentFile != null)
            {
                var folder = Path.Combine(WebHostEnvironment.WebRootPath, "attachments");
                var baseFileName = appointment.ProviderAvailabilityId + "student";
                var filesToDelete = Directory.GetFiles(folder)
                                              .Where(filePath => Path.GetFileNameWithoutExtension(filePath) == baseFileName);

                foreach (var fileToDelete in filesToDelete)
                {
                    System.IO.File.Delete(fileToDelete);
                }

                var attachmentName = baseFileName + Path.GetExtension(AppointmentModelInput.studentFile.FileName);
                var filePath = Path.Combine(folder, attachmentName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    AppointmentModelInput.studentFile.CopyTo(fileStream);
                }

                appointment.StudentAttachment = attachmentName;
            }
            _unitOfWork.Appointment.Update(appointment);
            _unitOfWork.Commit();

            return RedirectToPage("./Index");
        }

        public IActionResult OnPostAvailabilityDetails(string? id)
        {
            var appointment = _unitOfWork.Appointment.GetById(id);

            if (AvailabilityModelInput.providerFile != null)
            {
                var folder = Path.Combine(WebHostEnvironment.WebRootPath, "attachments");
                var baseFileName = appointment.ProviderAvailabilityId + "provider";
                var filesToDelete = Directory.GetFiles(folder)
                                              .Where(filePath => Path.GetFileNameWithoutExtension(filePath) == baseFileName);

                foreach (var fileToDelete in filesToDelete)
                {
                    System.IO.File.Delete(fileToDelete);
                }

                var attachmentName = baseFileName + Path.GetExtension(AvailabilityModelInput.providerFile.FileName);
                var filePath = Path.Combine(folder, attachmentName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    AvailabilityModelInput.providerFile.CopyTo(fileStream);
                }

                appointment.ProviderAttachment = attachmentName;
            }
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
                Campus = availability.Location.Campus,
                Location = availability.Location.LocationValue,
                Date = availability.StartTime.ToLongDateString(),
                Time = availability.StartTime.ToShortTimeString(),
                TimeDate = availability.StartTime
            };
            if (appointment != null)
            {
                var client = _unitOfWork.Client.Get(c => c.AppUserId == appointment.ClientId, includes: "AppUser");
                AvailabilityModelInput.WNumber = client.AppUser.WNumber;
                AvailabilityModelInput.Comments = appointment.StudentComments;
                AvailabilityModelInput.ClientName = client.AppUser.FullName;
                AvailabilityModelInput.ClientEmail = client.AppUser.Email;
                AvailabilityModelInput.IsAppointment = true;
                AvailabilityModelInput.StudentNoShow = appointment.StudentNoShow;
                AvailabilityModelInput.studentAttachment = appointment.StudentAttachment;
                AvailabilityModelInput.providerAttachment = appointment.ProviderAttachment;
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

        public async Task<IActionResult> OnPostNoShowToggleAsync(string id)
        {
            var appointment = _unitOfWork.Appointment.GetById(id);
            if (appointment.StudentNoShow)
                appointment.StudentNoShow = false;
            else appointment.StudentNoShow = true;
            _unitOfWork.Appointment.Update(appointment);

            await _unitOfWork.CommitAsync();

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