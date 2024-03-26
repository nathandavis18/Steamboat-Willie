using System;
using System.Collections.Generic;
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
        public IEnumerable<SelectListItem> ProviderList { get; set; }

        public IndexModel(UnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment, UserManager<AppUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
            objAvailability = new ProviderAvailability();
            ProviderList = new List<SelectListItem>();
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
                //var currentUserId = _userManager.GetUserId(User);
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
                        ProviderId = 1,
                        StartTime = combinedDateTime,
                        EndTime = endTime,
                        Duration = duration,
                        Scheduled = false
                    };

                    _unitOfWork.ProviderAvailability.Add(availabilitySlot);

                    // Update start and end time for the next appointment
                    combinedDateTime = endTime;
                    endTime = combinedDateTime.AddHours(duration.Hour).AddMinutes(duration.Minute);
                }
            }

            _unitOfWork.Commit();
            return RedirectToPage("./Index");

        }
    }
}
