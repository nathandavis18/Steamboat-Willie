using System;
using System.Collections.Generic;
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
using Utility;

namespace SteamboatWillieWeb.Pages.Availability
{
    public class IndexModel : PageModel
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;

        [BindProperty]
        public ProviderAvailability objAvailability { get; set; }
        [BindProperty]
        public Location objLocation { get; set; }
        public IEnumerable<SelectListItem> ProviderList { get; set; }
        public IEnumerable<Location> locations { get; set; }

        public IndexModel(UnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment, UserManager<AppUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
            objAvailability = new ProviderAvailability();
            objLocation = new Location();
            ProviderList = new List<SelectListItem>();
            locations = unitOfWork.Location.GetAll();
        }

        public IActionResult OnGet(int? id)
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

            return Page();
        }

        public IActionResult OnPost(int? id)
        {
            if (TempData.ContainsKey("error_message"))
            {
                // Error message exists, return the page without executing further logic
                return Page();
            }

            if (!ModelState.IsValid)
            {
                // Log or debug model state errors
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    Console.WriteLine($"Validation Error: {error.ErrorMessage}");
                }
                return Page();
            }

            if (ModelState.IsValid)
            {
                bool isRecurrenceChecked = Request.Form["recurrence"].Count > 0;
                bool isWeeklyChecked = Request.Form["weekly"].Count > 0;
                int count = 0;
                int weekCount = 0;

                if (isRecurrenceChecked)
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

                    var currentUserId = _userManager.GetUserId(User);
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
                            LocationId = 1
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
                        var endSelectedDate = reccurentDateTime.AddDays(6);
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
                                        LocationId = 1
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
                                            LocationId = 1
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
                                        LocationId = 1
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
                    var currentUserId = _userManager.GetUserId(User);
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
                            LocationId = 1
                        };

                        _unitOfWork.ProviderAvailability.Add(availabilitySlot);

                        // Update start and end time for the next appointment
                        combinedDateTime = endTime;
                        endTime = combinedDateTime.AddHours(duration.Hour).AddMinutes(duration.Minute);
                    }
                }
            }

            _unitOfWork.Commit();
            return RedirectToPage("./Index");

        }
    }
}
