using DataAccess;
using Google.Apis.Calendar.v3.Data;
using Infrastructure.Models;
using Infrastructure.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using SixLabors.ImageSharp.Processing;
using System;
using System.Linq;
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
            public string? TextColor {  get; set; }
        }

        private class xTypes
        {
            public string Text {  get; set; }
            public string Value {  get; set; }
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
        public FilterModel FilterModelInput { get; set; }

        public class FilterModel
        {
            public string? CurrentCampus { get; set; }
            public string? CurrentProvider { get; set; }
            public string? CurrentProviderType { get; set; }
            public string? CurrentAppointmentType { get; set; }
            public string? CurrentClasses { get; set; }

            public List<SelectListItem> Classes { get; set; } = new List<SelectListItem>();
            public List<SelectListItem> Providers { get; set; } = new List<SelectListItem>();
            public List<SelectListItem> AppointmentTypes { get; set; } = new List<SelectListItem>();
            public List<SelectListItem> Campuses { get; set; } = new List<SelectListItem>();
            public List<SelectListItem> ProviderTypes { get; set; } = new List<SelectListItem>();
        }

        public IActionResult OnGet(string? campus, string? providerId, string? providerType, string? appointmentType, string? cls)
        {
            if (!User.Identity!.IsAuthenticated)
            {
                return RedirectToPage("/Account/Login", new { ReturnUrl = "/Appointment/Index", Area = "Identity" });
            }
            var user = _userManager.GetUserAsync(User).GetAwaiter().GetResult();
            var client = _unitOfWork.Client.GetById(user.Id);
            if (!User.IsInRole(SD.CLIENT_ROLE))
            {
                TempData["error"] = "Access Denied. If you believe you should have access, report this to the administrator.";
                return RedirectToPage("../Index");
            }

            FilterModelInput.CurrentCampus = campus;
            FilterModelInput.CurrentProvider = providerId;
            FilterModelInput.CurrentClasses = cls;
            FilterModelInput.CurrentProviderType = providerType;
            FilterModelInput.CurrentAppointmentType = appointmentType;

            var providers = _unitOfWork.Provider.GetAll(includes: "AppUser").ToList();
            var classes = _unitOfWork.ProviderClass.GetAll(includes: "Provider,Class").Where(c => c.Class.IsDisabled != true).ToList();
            var availabilities = _unitOfWork.ProviderAvailability.GetAll(a => !a.Scheduled && a.ProviderId != user.Id && a.StartTime >= DateTime.Today, includes: "Provider,Location").ToList();
            if (!client.StudentType.Contains("Flex"))
            {
                availabilities = availabilities.Where(a => !a.AppointmentType.Contains("Advising for Flex Students")).ToList();
            }

            if (!client.IsWeberStudent)
            {
                availabilities = availabilities.Where(a => a.AppointmentType.Contains("General Advising") || a.AppointmentType.Contains("Advising for New Students")).ToList();
            }

            var appointmentTypes = new List<xTypes>
            {
                new xTypes { Text = "General Advising", Value = String.Empty },
                new xTypes {Text = "New Student Advising", Value = "Advising for New Students"}
            };
            if (client.IsWeberStudent)
            {
                appointmentTypes.Add(new xTypes { Text = "Tutoring", Value = "Tutoring" });
                appointmentTypes.Add(new xTypes { Text = "Current Student Advising", Value = "Advising for Current Students" });
                appointmentTypes.Add(new xTypes { Text = "Office Hours", Value = "Office Hours" });
            }
            if (client.StudentType.Contains("Flex"))
            {
                appointmentTypes.Add(new xTypes { Text = "Flex Student Advising", Value = "Advising for Flex Students" }); //Only flex students can get this option.
            }
            var providerTypes = new List<xTypes>
            {
                new xTypes { Text = "Advisor", Value = "Advisor"}
            };
            if (client.IsWeberStudent)
            {
                providerTypes.Add(new xTypes { Text = "Tutor", Value = "Tutor" });
                providerTypes.Add(new xTypes { Text = "Instructor", Value = "Instructor" });
            }
            else
            {
                providers = providers.Where(p => p.Title.Equals("Advisor")).ToList();
                classes.Clear();
            }

            if (!String.IsNullOrEmpty(providerType))
            {
                switch (providerType)
                {
                    case "Instructor":
                        appointmentTypes = appointmentTypes.Where(a => a.Text.Contains("Office Hours")).ToList();
                        break;
                    case "Advisor":
                        appointmentTypes = appointmentTypes.Where(a => a.Text.Contains("Advising")).ToList();
                        classes.Clear();
                        break;
                    case "Tutor":
                        appointmentTypes = appointmentTypes.Where(a => a.Text.Contains("Tutoring")).ToList();
                        break;
                    default:
                        break;
                }
                providers = providers.Where(p => p.Title.Equals(providerType)).ToList();
                availabilities = availabilities.Where(a => a.Provider.Title.Equals(providerType)).ToList();
            }

            if (!String.IsNullOrEmpty(appointmentType))
            {
                switch (appointmentType)
                {
                    case "Office Hours":
                        classes = classes.Where(c => c.Provider.Title.Equals("Instructor")).ToList();
                        break;
                    case "Tutoring":
                        classes = classes.Where(c => c.Provider.Title.Equals("Tutor")).ToList();
                        break;
                    default:
                        classes.Clear();
                        break;
                }
                availabilities = availabilities.Where(a => a.AppointmentType.Contains(appointmentType)).ToList();
            }

            if (!String.IsNullOrEmpty(campus))
            {
                availabilities = availabilities.Where(a => a.Location.Campus.Equals(campus)).ToList();
            }
            if (!String.IsNullOrEmpty(providerId))
            {
                var provider = _unitOfWork.Provider.GetById(providerId);
                FilterModelInput.AppointmentTypes.Clear();
                FilterModelInput.ProviderTypes.Clear();
                switch (provider.Title)
                {
                    case "Tutor":
                        appointmentTypes = appointmentTypes.Where(a => a.Text.Contains("Tutoring")).ToList();
                        break;
                    case "Instructor":
                        appointmentTypes = appointmentTypes.Where(a => a.Text.Contains("Office Hours")).ToList();
                        break;
                    case "Advisor":
                        appointmentTypes = appointmentTypes.Where(a => a.Text.Contains("Advising")).ToList();
                        classes.Clear();
                        break;
                }
                providerTypes = providerTypes.Where(p => p.Text.Equals(provider.Title)).ToList();
                classes = classes.Where(c => c.ProviderId.Equals(providerId)).ToList();
                availabilities = availabilities.Where(a => a.ProviderId.Equals(providerId)).ToList();
            }
            if (!String.IsNullOrEmpty(cls))
            {
                availabilities = availabilities.Where(a => a.AppointmentType.Contains(cls)).ToList();
            }
            providers = providers.OrderBy(x => x.AppUser.LName).ThenBy(x => x.AppUser.FName).ToList();
            classes = classes.OrderBy(x => x.Class.Name).ToList();

            FilterModelInput.Classes = classes.Select(x => new SelectListItem
            {
                Text = x.Class.Name,
                Value = x.Class.Name
            }).DistinctBy(x => x.Value).ToList();


            FilterModelInput.Providers = providers.Select(x => new SelectListItem
            {
                Text = x.AppUser.FullName + " - " + _unitOfWork.Department.GetById(x.DepartmentId).DepartmentName,
                Value = x.AppUserId
            }).ToList();

            FilterModelInput.Campuses = new List<SelectListItem>()
            {
                new SelectListItem { Text = "Ogden Campus", Value = "Ogden" },
                new SelectListItem {Text = "Davis Campus", Value = "Davis"},
                new SelectListItem { Text = "SLCC Campus", Value = "SLCC"},
                new SelectListItem {Text = "Online", Value = "Online"}
            };

            FilterModelInput.ProviderTypes = providerTypes.Select(x => new SelectListItem
            {
                Text = x.Text,
                Value = x.Value
            }).ToList();

            FilterModelInput.AppointmentTypes = appointmentTypes.Select(x => new SelectListItem
            {
                Text = x.Text,
                Value = x.Value
            }).ToList();

            foreach (var a in availabilities)
            {
                CalendarObj.Add(new Calendar
                {
                    Id = a.Id,
                    Name = _unitOfWork.AppUser.GetById(a.ProviderId).FullName,
                    StartTime = DateTimeParser.ParseDateTime(a.StartTime),
                    EndTime = DateTimeParser.ParseDateTime(a.EndTime),
                    Color = a.Provider.HexColor,
                    TextColor = a.Provider.HexColor != null ? GetTextColor(a.Provider.HexColor) : "#FFFFFF"
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
                ProviderName = _unitOfWork.AppUser.GetById(provider.AppUserId).FullName,
                Campus = _unitOfWork.Location.GetById(availability.LocationId).Campus
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
                Event @event = EventCreater.CreateEvent(summary, location, appointment.Description, availability.StartTime.ToString(), availability.EndTime.ToString());
                string clientCalendarId = await _googleCalendarService.AddEvent(@event, clientId, new CancellationToken(false));
                appointment.ClientEventId = clientCalendarId;
            }

            if (_unitOfWork.AppUser.GetById(providerId).GoogleCalendarIntegration.Value)
            {
                string summary = availability.AppointmentType + " with " + _unitOfWork.AppUser.GetById(clientId).FullName;
                Event @event = EventCreater.CreateEvent(summary, location, appointment.Description, availability.StartTime.ToString(), availability.EndTime.ToString());
                string providerCalendarId = await _googleCalendarService.AddEvent(@event, providerId, new CancellationToken(false));
                appointment.ProviderEventId = providerCalendarId;
            }

            _unitOfWork.Appointment.Add(appointment);
            await _unitOfWork.CommitAsync();


            return RedirectToPage("../Index");
        }

        private string GetTextColor(string hexCode)
        {
            string textColor = "#FFFFFF";
            double brightness = 0;

            int red = 0, blue = 0, green = 0;

            red += Convert.ToInt32(hexCode.Substring(1, 2).ToUpper(), 16);
            green += Convert.ToInt32(hexCode.Substring(3, 2).ToUpper(), 16);
            blue += Convert.ToInt32(hexCode.Substring(5, 2).ToUpper(), 16);

            brightness = 0.299 * red + 0.587 * green + 0.114 * blue;
            if(brightness > 128)
            {
                textColor = "#000000";
            }

            return textColor;
        }
    }
}
