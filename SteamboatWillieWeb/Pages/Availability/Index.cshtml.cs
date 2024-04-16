using DataAccess;
using Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using Utility;

namespace SteamboatWillieWeb.Pages.Availability
{
    public class IndexModel : PageModel
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        public string? CurrentUserTitle;

        [BindProperty]
        public IEnumerable<SelectListItem>? Locations { get; set; }

        [BindProperty]
        public IEnumerable<SelectListItem>? Classes { get; set; }

        public IndexModel(UnitOfWork unitOfWork, UserManager<AppUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }
        [BindProperty]
        public RecurrenceModel RecurrenceModelInput { get; set; }

        [BindProperty]
        public AvailabilityModel AvailabilityModelInput { get; set; }

        public class AvailabilityModel
        {
            [Required]
            public string ProviderId {  get; set; }

            [Required]
            [Display(Name = "Start Date")]
            [DataType(DataType.Date)]
            [DateValidator(ErrorMessage = "The Date must be later than today")]
            public DateTime StartDate { get; set; }

            [Required]
            [Display(Name = "Appointment Duration")]
            public DateTime Duration { get; set; }

            [Required]
            [Display(Name = "Number of Appointments")]
            [Range(1, 10, ErrorMessage = "Minimum 1, Max 10")]
            public short NumAppointments {  get; set; }

            [Required]
            [Display(Name = "Start Time")]
            [DataType(DataType.Time)]
            public TimeSpan StartTime { get; set; }

            [Required]
            [Display(Name = "Class")]
            public string? AppointmentType { get; set; }

            [Required]
            [Display(Name = "Location")]
            public string LocationId {  get; set; }
            public string? Location { get; set; }

            public bool NewLocation { get; set; }

            public string? Campus { get; set; }
        }

        public class RecurrenceModel
        {

            [Required(ErrorMessage = "One or more days must be selected")]
            [Display(Name = "Recurrence")]
            public bool Recurrence { get; set; }

            [Display(Name = "Sunday")]
            public bool Sunday {  get; set; }
            [Display(Name = "Monday")]
            public bool Monday { get; set; }
            [Display(Name = "Tuesday")]
            public bool Tuesday {  get; set; }
            [Display(Name = "Wednesday")]
            public bool Wednesday { get; set; }
            [Display(Name = "Thursday")]
            public bool Thursday { get; set; }
            [Display(Name = "Friday")]
            public bool Friday { get; set; }
            [Display(Name = "Saturday")]
            public bool Saturday { get; set; }

            [Display(Name = "Recurrence End")]
            [DataType(DataType.Date)]
            public DateTime EndDate { get; set; }

            [Display(Name = "Weekly: ")]
            public bool IsWeekly {  get; set; }
        }

        public IActionResult OnGet(int? id, string? date)
        {
            var currentUserId = _userManager.GetUserId(User);
            var currentUser = _unitOfWork.Provider.Get(p => p.AppUserId == currentUserId);
            CurrentUserTitle = currentUser.Title;

            if (!User.Identity!.IsAuthenticated)
            {
                return RedirectToPage("/Account/Login", new { ReturnUrl = "/Availability/Index", Area = "Identity" });
            }
            if (!User.IsInRole(SD.PROVIDER_ROLE))
            {
                TempData["error"] = "Access Denied. If you believe you should have access, report this to the administrator.";
                return RedirectToPage("../Index");
            }

            var user = _userManager.GetUserAsync(User).GetAwaiter().GetResult();
            var providerLocations = _unitOfWork.ProviderAvailability
                .GetAll()
                .Where(pa => pa.ProviderId == user.Id)
                .Select(pa => pa.LocationId)
                .Distinct()
                .ToList();
            Locations = _unitOfWork.Location
                .GetAll()
                .Where(l => providerLocations.Contains(l.Id))
                .Select(l => new SelectListItem { Value = l.Id.ToString(), Text = l.LocationValue })
                .ToList();

            var providerClasses = _unitOfWork.ProviderClass
                .GetAll()
                .Where(pc => pc.ProviderId == user.Id)
                .Select(pc => pc.ClassId)
                .Distinct()
                .ToList();
            Classes = _unitOfWork.Class
                .GetAll()
                .Where(c => providerClasses.Contains(c.Id))
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToList();

            DateTime startDate = DateTime.Now.AddDays(1);
            if(date != null)
            {
                startDate = DateTime.Parse(date);
            }

            RecurrenceModelInput = new RecurrenceModel()
            {
                EndDate = startDate.AddDays(7)
            };
            AvailabilityModelInput = new AvailabilityModel()
            {
                StartDate = startDate,
                StartTime = _unitOfWork.Provider.GetById(user.Id).StartTime.Value.TimeOfDay,
                ProviderId = user.Id,
                NumAppointments = 1,
                NewLocation = true
            };

            return Page();
        }

        public IActionResult OnPost(int? id)
        {
            bool makeLocation = false;
            var currentUserId = _userManager.GetUserId(User);
            var provider = _unitOfWork.Provider.GetById(currentUserId);
            if (AvailabilityModelInput.StartTime < provider.StartTime.Value.TimeOfDay)
            {
                ModelState.AddModelError("AvailabilityModelInput.StartTime", "Start Time cannot be before office hours start");
            }
            else
            {
                for (int i = 1; i <= AvailabilityModelInput.NumAppointments; ++i)
                {
                    TimeSpan meetingEnd = new TimeSpan(AvailabilityModelInput.StartTime.Ticks + (AvailabilityModelInput.Duration.TimeOfDay.Ticks * i));
                    if (meetingEnd > provider.EndTime.Value.TimeOfDay)
                    {
                        ModelState.AddModelError("AvailabilityModelInput.StartTime", "All meetings must end before office hours end");
                    }
                }
            }

            if(int.Parse(AvailabilityModelInput.LocationId) == 0)
            {
                if (AvailabilityModelInput.Location.IsNullOrEmpty())
                {
                    ModelState.AddModelError("AvailabilityModelInput.Location", "You must enter a value for the new location");
                }
                else
                {
                    makeLocation = true;
                }
            }

            if (RecurrenceModelInput.Recurrence)
            {
                if(!(RecurrenceModelInput.Monday || RecurrenceModelInput.Tuesday || RecurrenceModelInput.Wednesday || RecurrenceModelInput.Thursday || RecurrenceModelInput.Friday || RecurrenceModelInput.Saturday || RecurrenceModelInput.Sunday))
                {
                    ModelState.AddModelError("RecurrenceModelInput.Recurrence", "At least one day must be selected");
                }
                else if(RecurrenceModelInput.IsWeekly)
                {
                    if(RecurrenceModelInput.EndDate < AvailabilityModelInput.StartDate.AddDays(7))
                    {
                        ModelState.AddModelError("RecurrenceModelInput.EndDate", "End Date must be at least a week after start date");
                    }
                }

            }
            if (!ModelState.IsValid)
            {
                // Log or debug model state errors
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    Console.WriteLine($"Validation Error: {error.ErrorMessage}");
                }

                var providerLocations = _unitOfWork.ProviderAvailability
                    .GetAll()
                    .Where(pa => pa.ProviderId == currentUserId)
                    .Select(pa => pa.LocationId)
                    .Distinct()
                    .ToList();
                Locations = _unitOfWork.Location
                    .GetAll()
                    .Where(l => providerLocations.Contains(l.Id))
                    .Select(l => new SelectListItem { Value = l.Id.ToString(), Text = l.LocationValue })
                    .ToList();

                return Page();
            }

            if (makeLocation)
            {
                Location newLocation = new Location()
                {
                    LocationValue = AvailabilityModelInput.Location,
                    Campus = AvailabilityModelInput.Campus
                };
                _unitOfWork.Location.Add(newLocation);
                _unitOfWork.Commit();
                AvailabilityModelInput.LocationId = newLocation.Id.ToString();
            }

            if(CurrentUserTitle == "Instructor")
            { 
                if (AvailabilityModelInput.AppointmentType != "Office Hours")
                {
                    AvailabilityModelInput.AppointmentType = "Office Hours for " + _unitOfWork.Class.GetById(int.Parse(AvailabilityModelInput.AppointmentType)).Name;
                }
            }
            else if(CurrentUserTitle == "Tutor")
            {
                AvailabilityModelInput.AppointmentType = "Tutoring for " + _unitOfWork.Class.GetById(int.Parse(AvailabilityModelInput.AppointmentType)).Name;
            }
            else
            {
                if(AvailabilityModelInput.AppointmentType != "General Advising")
                {
                    AvailabilityModelInput.AppointmentType = "Advising for " + AvailabilityModelInput.AppointmentType;
                }
            }

            DateTime startDate = AvailabilityModelInput.StartDate + AvailabilityModelInput.StartTime;
            if (!RecurrenceModelInput.Recurrence)
            {
                for (int i = 1; i <= AvailabilityModelInput.NumAppointments; ++i)
                {
                    ProviderAvailability pa = new ProviderAvailability
                    {
                        ProviderId = AvailabilityModelInput.ProviderId,
                        LocationId = int.Parse(AvailabilityModelInput.LocationId),
                        StartTime = startDate + (AvailabilityModelInput.Duration.TimeOfDay * (i - 1)),
                        EndTime = startDate.AddHours(AvailabilityModelInput.Duration.Hour).AddMinutes(AvailabilityModelInput.Duration.Minute) + (AvailabilityModelInput.Duration.TimeOfDay * (i - 1)),
                        Duration = AvailabilityModelInput.Duration,
                        Scheduled = false,
                        AppointmentType = AvailabilityModelInput.AppointmentType
                    };
                    _unitOfWork.ProviderAvailability.Add(pa);
                }
            }
            else
            {
                List<string> daysOfWeek = DaysOfWeek();
                if (RecurrenceModelInput.IsWeekly)
                {
                    for (var x = startDate; x <= RecurrenceModelInput.EndDate.AddDays(1); x = x.AddDays(1))
                    {
                        if (daysOfWeek.Contains(x.DayOfWeek.ToString()))
                        {
                            for (int i = 1; i <= AvailabilityModelInput.NumAppointments; ++i)
                            {
                                ProviderAvailability pa = new ProviderAvailability
                                {
                                    ProviderId = AvailabilityModelInput.ProviderId,
                                    LocationId = int.Parse(AvailabilityModelInput.LocationId),
                                    StartTime = x + (AvailabilityModelInput.Duration.TimeOfDay * (i - 1)),
                                    EndTime = x.AddHours(AvailabilityModelInput.Duration.Hour).AddMinutes(AvailabilityModelInput.Duration.Minute) + (AvailabilityModelInput.Duration.TimeOfDay * (i - 1)),
                                    Duration = AvailabilityModelInput.Duration,
                                    Scheduled = false,
                                    AppointmentType = AvailabilityModelInput.AppointmentType
                                };
                                _unitOfWork.ProviderAvailability.Add(pa);
                            }
                        }
                    }
                }
                else
                {
                    for(var x = startDate; x <= startDate.AddDays(6); x = x.AddDays(1))
                    {
                        if (daysOfWeek.Contains(x.DayOfWeek.ToString()))
                        {
                            for (int i = 1; i <= AvailabilityModelInput.NumAppointments; ++i)
                            {
                                ProviderAvailability pa = new ProviderAvailability
                                {
                                    ProviderId = AvailabilityModelInput.ProviderId,
                                    LocationId = int.Parse(AvailabilityModelInput.LocationId),
                                    StartTime = x + (AvailabilityModelInput.Duration.TimeOfDay * (i - 1)),
                                    EndTime = x.AddHours(AvailabilityModelInput.Duration.Hour).AddMinutes(AvailabilityModelInput.Duration.Minute) + (AvailabilityModelInput.Duration.TimeOfDay * (i - 1)),
                                    Duration = AvailabilityModelInput.Duration,
                                    Scheduled = false,
                                    AppointmentType = AvailabilityModelInput.AppointmentType
                                };
                                _unitOfWork.ProviderAvailability.Add(pa);
                            }
                        }
                    }
                }
            }

            _unitOfWork.Commit();
            return RedirectToPage("../Index");

            /**
             * Previous Code from this section can be found at the bottom of this page.
             */
        }

        private List<string> DaysOfWeek()
        {
            List<string> daysOfWeek = new List<string>();
            if (RecurrenceModelInput.Monday)
            {
                daysOfWeek.Add("Monday");
            }
            if (RecurrenceModelInput.Tuesday)
            {
                daysOfWeek.Add("Tuesday");
            }
            if (RecurrenceModelInput.Wednesday)
            {
                daysOfWeek.Add("Wednesday");
            }
            if (RecurrenceModelInput.Thursday)
            {
                daysOfWeek.Add("Thursday");
            }
            if (RecurrenceModelInput.Friday)
            {
                daysOfWeek.Add("Friday");
            }
            if (RecurrenceModelInput.Saturday)
            {
                daysOfWeek.Add("Saturday");
            }
            if (RecurrenceModelInput.Sunday)
            {
                daysOfWeek.Add("Sunday");
            }
            return daysOfWeek;
        }
    }
}


//Previous OnPost functionality

/*if (isRecurrenceChecked)
            {
                var selectedDays = Request.Form["weekDays[]"];
                List<string> selectedDaysList = new List<string>();

                if (selectedDays.Any())
                {
                    foreach (var day in selectedDays)
                    {
                        selectedDaysList.Add(day);
                    }
                }

                DateTime startDate = DateTime.Parse(Request.Form["startDate"]);
                DateTime startTime = DateTime.Parse(Request.Form["startTime"]);
                DateTime combinedDateTime = startDate.Date + startTime.TimeOfDay;
                DateTime reccurentDateTime = combinedDateTime;
                DateTime duration = DateTime.Parse(Request.Form["objAvailability.Duration"]);
                int numAppointments = int.Parse(Request.Form["numAppointments"]);
                DateTime endTime = combinedDateTime.AddHours(duration.Hour).AddMinutes(duration.Minute);
                DateTime endDate = DateTime.Parse(Request.Form["endDate"]);
                string weekDay = reccurentDateTime.DayOfWeek.ToString();
                string location = Request.Form["location"];

                for (int i = 0; i < numAppointments; i++)
                {
                    ProviderAvailability availabilitySlot = new ProviderAvailability
                    {
                        ProviderId = currentUserId,
                        StartTime = reccurentDateTime,
                        EndTime = endTime,
                        Duration = duration,
                        Scheduled = false,
                        LocationId = objAvailability.LocationId
                    };

                    _unitOfWork.ProviderAvailability.Add(availabilitySlot);

                    // Update start and end time for the next appointment
                    reccurentDateTime = endTime;
                    endTime = reccurentDateTime.AddHours(duration.Hour).AddMinutes(duration.Minute);
                }

                reccurentDateTime = combinedDateTime;
                reccurentDateTime = reccurentDateTime.AddDays(1);
                weekDay = reccurentDateTime.DayOfWeek.ToString();
                endTime = reccurentDateTime.AddHours(duration.Hour).AddMinutes(duration.Minute);

                if (selectedDaysList.Any())
                {
                    var endSelectedDate = reccurentDateTime.AddDays(5);
                    while (reccurentDateTime <= endSelectedDate)
                    {
                        count++;
                        if (selectedDaysList.Contains(weekDay))
                        {
                            for (int i = 0; i < numAppointments; i++)
                            {
                                ProviderAvailability availabilitySlot = new ProviderAvailability
                                {
                                    ProviderId = currentUserId,
                                    StartTime = reccurentDateTime,
                                    EndTime = endTime,
                                    Duration = duration,
                                    Scheduled = false,
                                    LocationId = objAvailability.LocationId
                                };

                                _unitOfWork.ProviderAvailability.Add(availabilitySlot);

                                // Update start and end time for the next appointment
                                reccurentDateTime = endTime;
                                endTime = reccurentDateTime.AddHours(duration.Hour).AddMinutes(duration.Minute);
                            }
                        }
                        reccurentDateTime = combinedDateTime.AddDays(count);
                        reccurentDateTime.AddDays(1);
                        weekDay = reccurentDateTime.DayOfWeek.ToString();
                        endTime = reccurentDateTime.AddHours(duration.Hour).AddMinutes(duration.Minute);
                    }
                }

                if (isWeeklyChecked)
                {
                    weekCount = 7;
                    reccurentDateTime = combinedDateTime.AddDays(weekCount);
                    endTime = reccurentDateTime.AddHours(duration.Hour).AddMinutes(duration.Minute);


                    if (selectedDaysList.Any())
                    {
                        while (reccurentDateTime <= endDate)
                        {
                            reccurentDateTime = combinedDateTime.AddDays(count);
                            endTime = reccurentDateTime.AddHours(duration.Hour).AddMinutes(duration.Minute);
                            count++;
                            if (selectedDaysList.Contains(weekDay))
                            {
                                for (int i = 0; i < numAppointments; i++)
                                {
                                    ProviderAvailability availabilitySlot = new ProviderAvailability
                                    {
                                        ProviderId = currentUserId,
                                        StartTime = reccurentDateTime,
                                        EndTime = endTime,
                                        Duration = duration,
                                        Scheduled = false,
                                        LocationId = objAvailability.LocationId
                                    };

                                    _unitOfWork.ProviderAvailability.Add(availabilitySlot);

                                    // Update start and end time for the next appointment
                                    reccurentDateTime = endTime;
                                    endTime = reccurentDateTime.AddHours(duration.Hour).AddMinutes(duration.Minute);
                                }
                            }
                            reccurentDateTime = combinedDateTime.AddDays(count);
                            reccurentDateTime.AddDays(1);
                            weekDay = reccurentDateTime.DayOfWeek.ToString();
                            endTime = reccurentDateTime.AddHours(duration.Hour).AddMinutes(duration.Minute);
                        }
                    }
                    else
                    {
                        while (reccurentDateTime <= endDate)
                        {
                            for (int i = 0; i < numAppointments; i++)
                            {
                                ProviderAvailability availabilitySlot = new ProviderAvailability
                                {
                                    ProviderId = currentUserId,
                                    StartTime = reccurentDateTime,
                                    EndTime = endTime,
                                    Duration = duration,
                                    Scheduled = false,
                                    LocationId = objAvailability.LocationId
                                };

                                _unitOfWork.ProviderAvailability.Add(availabilitySlot);

                                // Update start and end time for the next appointment
                                reccurentDateTime = endTime;
                                endTime = reccurentDateTime.AddHours(duration.Hour).AddMinutes(duration.Minute);
                            }

                            weekCount += 7;
                            reccurentDateTime = combinedDateTime.AddDays(weekCount);
                            endTime = reccurentDateTime.AddHours(duration.Hour).AddMinutes(duration.Minute);
                        }
                    }
                }
            }
            else
            {
                DateTime startDate = DateTime.Parse(Request.Form["startDate"]);
                DateTime startTime = DateTime.Parse(Request.Form["startTime"]);
                DateTime combinedDateTime = startDate.Date + startTime.TimeOfDay;
                DateTime duration = DateTime.Parse(Request.Form["objAvailability.Duration"]);

                int numAppointments = int.Parse(Request.Form["numAppointments"]);
                DateTime endTime = combinedDateTime.AddHours(duration.Hour).AddMinutes(duration.Minute);

                for (int i = 0; i < numAppointments; i++)
                {
                    ProviderAvailability availabilitySlot = new ProviderAvailability
                    {
                        ProviderId = currentUserId,
                        StartTime = combinedDateTime,
                        EndTime = endTime,
                        Duration = duration,
                        Scheduled = false,
                        LocationId = objAvailability.LocationId
                    };

                    _unitOfWork.ProviderAvailability.Add(availabilitySlot);

                    // Update start and end time for the next appointment
                    combinedDateTime = endTime;
                    endTime = combinedDateTime.AddHours(duration.Hour).AddMinutes(duration.Minute);
                }
            }

        _unitOfWork.Commit();
        return RedirectToPage("../Index");*/