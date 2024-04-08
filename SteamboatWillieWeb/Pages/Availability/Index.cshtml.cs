using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using DataAccess;
using Infrastructure.Interfaces;
using Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.IdentityModel.Tokens;
using Utility;

namespace SteamboatWillieWeb.Pages.Availability
{
    public class IndexModel : PageModel
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        public List<SelectListItem> DropdownOptions { get; set; }
        public string CurrentUserStartTime { get; set; }
        public string CurrentUserEndTime { get; set; }

        [BindProperty]
        public ProviderAvailability objAvailability { get; set; }
        [BindProperty]
        public Location objLocation { get; set; }
        public IEnumerable<SelectListItem> ProviderList { get; set; }
        public IEnumerable<int> Locations { get; set; }
        public string PassedDate { get; set; }

        public IndexModel(UnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment, UserManager<AppUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
            objAvailability = new ProviderAvailability();
            objLocation = new Location();
            ProviderList = new List<SelectListItem>();
            Locations = new List<int>();
        }
        [BindProperty]
        public Test TestInput { get; set; }

        [BindProperty]
        public TestDate TestDateInput { get; set; }

        public class TestDate
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
            [Range(1.0, 10.0, ErrorMessage = "Minimum 1, Max 10")]
            public short NumAppointments {  get; set; }

            [Required]
            [Display(Name = "Start Time")]
            [DataType(DataType.Time)]
            public TimeSpan StartTime { get; set; }


            [Required]
            [Display(Name = "Location")]
            public string LocationId {  get; set; }
            public string Location { get; set; }

            public IEnumerable<SelectListItem>? Locations { get; set; }
        }

        public class Test
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
            if (!User.Identity!.IsAuthenticated)
            {
                return RedirectToPage("/Account/Login", new { ReturnUrl = "/Availability/Index", Area = "Identity" });
            }
            if (!User.IsInRole(SD.PROVIDER_ROLE))
            {
                TempData["access_denied"] = "Access Denied. If you believe you should have access, report this to the administrator.";
                return RedirectToPage("../Index");
            }

            if (date != null)
            {
                PassedDate = date;
            }

            var currentUserId = _userManager.GetUserId(User);
            TestInput = new Test()
            {
                EndDate = DateTime.Now.AddDays(8)
            };
            TestDateInput = new TestDate()
            {
                StartDate = DateTime.Now.AddDays(1),
                ProviderId = currentUserId
            };
                

            Locations = _unitOfWork.ProviderAvailability
                .GetAll()
                .Where(pa => pa.ProviderId == currentUserId)
                .Select(pa => pa.LocationId)
                .Distinct()
                .ToList();

            TestDateInput.Locations = _unitOfWork.Location
                .GetAll()
                .Where(l => Locations.Contains(l.Id))
                .Select(selector: l => new SelectListItem { Value = l.Id.ToString(), Text = l.LocationValue })
                .ToList();


            var currentUser = _unitOfWork.Provider.Get(p => p.AppUserId == currentUserId, false, "Department");
            if (currentUser != null)
            {
                CurrentUserStartTime = currentUser.StartTime?.AddSeconds(-1).ToString("HH:mm:ss") ?? "07:00:00"; // Default to 07:00:00 if start time is null
                CurrentUserEndTime = currentUser.EndTime?.ToString("HH:mm:ss") ?? "19:00:00"; // Default to 19:00:00 if end time is null
            }

            return Page();
        }

        public IActionResult OnPost(int? id)
        {
            var currentUserId = _userManager.GetUserId(User);
            var provider = _unitOfWork.Provider.Get(p => p.AppUserId == currentUserId);
            if (TestDateInput.StartTime < provider.StartTime.Value.TimeOfDay)
            {
                ModelState.AddModelError("TestDateInput.StartTime", "Start Time cannot be before office hours start");
            }
            else
            {
                for (int i = 1; i <= TestDateInput.NumAppointments; ++i)
                {
                    TimeSpan meetingEnd = new TimeSpan(TestDateInput.StartTime.Ticks + (TestDateInput.Duration.TimeOfDay.Ticks * i));
                    if (meetingEnd > provider.EndTime.Value.TimeOfDay)
                    {
                        ModelState.AddModelError("TestDateInput.StartTime", "All meetings must end before office hours end");
                    }
                }
            }

            if(int.Parse(TestDateInput.LocationId) == 0)
            {
                if (TestDateInput.Location.IsNullOrEmpty())
                {
                    ModelState.AddModelError("TestDateInput.Location", "You must enter a value for the new location");
                }
            }

            if (TestInput.Recurrence)
            {
                if(!(TestInput.Monday || TestInput.Tuesday || TestInput.Wednesday || TestInput.Thursday || TestInput.Friday || TestInput.Saturday || TestInput.Sunday))
                {
                    ModelState.AddModelError("TestInput.Recurrence", "At least one day must be selected");
                }
                else if(TestInput.IsWeekly)
                {
                    if(TestInput.EndDate < TestDateInput.StartDate.AddDays(7))
                    {
                        ModelState.AddModelError("TestInput.EndDate", "End Date must be at least a week after start date");
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

                Locations = _unitOfWork.ProviderAvailability
                   .GetAll()
                   .Where(pa => pa.ProviderId == currentUserId)
                   .Select(pa => pa.LocationId)
                   .Distinct()
                   .ToList();

                TestDateInput.Locations = _unitOfWork.Location
                    .GetAll()
                    .Where(l => Locations.Contains(l.Id))
                    .Select(selector: l => new SelectListItem { Value = l.Id.ToString(), Text = l.LocationValue })
                    .ToList();

                return Page();
            }


            DateTime startDate = TestDateInput.StartDate + TestDateInput.StartTime;
            if (!TestInput.Recurrence)
            {
                for (int i = 1; i <= TestDateInput.NumAppointments; ++i)
                {
                    ProviderAvailability pa = new ProviderAvailability
                    {
                        ProviderId = TestDateInput.ProviderId,
                        LocationId = int.Parse(TestDateInput.LocationId),
                        StartTime = startDate + (TestDateInput.Duration.TimeOfDay * (i - 1)),
                        EndTime = startDate.AddHours(startDate.Hour).AddMinutes(startDate.Minute) + (TestDateInput.Duration.TimeOfDay * (i - 1)),
                        Duration = TestDateInput.Duration,
                        Scheduled = false
                    };
                    _unitOfWork.ProviderAvailability.Add(pa);
                }
            }
            else
            {
                List<string> daysOfWeek = DaysOfWeek();
                if (TestInput.IsWeekly)
                {
                    for (var x = startDate; x <= TestInput.EndDate; x = x.AddDays(1))
                    {
                        if (daysOfWeek.Contains(x.DayOfWeek.ToString()))
                        {
                            for (int i = 1; i <= TestDateInput.NumAppointments; ++i)
                            {
                                ProviderAvailability pa = new ProviderAvailability
                                {
                                    ProviderId = TestDateInput.ProviderId,
                                    LocationId = int.Parse(TestDateInput.LocationId),
                                    StartTime = x + (TestDateInput.Duration.TimeOfDay * (i - 1)),
                                    EndTime = x.AddHours(x.Hour).AddMinutes(x.Minute) + (TestDateInput.Duration.TimeOfDay * (i - 1)),
                                    Duration = TestDateInput.Duration,
                                    Scheduled = false
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
                            for (int i = 1; i <= TestDateInput.NumAppointments; ++i)
                            {
                                ProviderAvailability pa = new ProviderAvailability
                                {
                                    ProviderId = TestDateInput.ProviderId,
                                    LocationId = int.Parse(TestDateInput.LocationId),
                                    StartTime = x + (TestDateInput.Duration.TimeOfDay * (i - 1)),
                                    EndTime = x.AddHours(x.Hour).AddMinutes(x.Minute) + (TestDateInput.Duration.TimeOfDay * (i - 1)),
                                    Duration = TestDateInput.Duration,
                                    Scheduled = false
                                };
                                _unitOfWork.ProviderAvailability.Add(pa);
                            }
                        }
                    }
                }
            }

            _unitOfWork.Commit();
            return RedirectToPage("../Index");

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

        }

        private List<string> DaysOfWeek()
        {
            List<string> daysOfWeek = new List<string>();
            if (TestInput.Monday)
            {
                daysOfWeek.Add("Monday");
            }
            if (TestInput.Tuesday)
            {
                daysOfWeek.Add("Tuesday");
            }
            if (TestInput.Wednesday)
            {
                daysOfWeek.Add("Wednesday");
            }
            if (TestInput.Thursday)
            {
                daysOfWeek.Add("Thursday");
            }
            if (TestInput.Friday)
            {
                daysOfWeek.Add("Friday");
            }
            if (TestInput.Saturday)
            {
                daysOfWeek.Add("Saturday");
            }
            if (TestInput.Sunday)
            {
                daysOfWeek.Add("Sunday");
            }
            return daysOfWeek;
        }
    }
}
