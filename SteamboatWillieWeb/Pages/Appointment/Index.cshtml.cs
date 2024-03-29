using DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.IdentityModel.Tokens;
using Utility;

namespace SteamboatWillieWeb.Pages.Appointment
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
        }

        private readonly UnitOfWork _unitOfWork;
        public IndexModel(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            CalendarObj = new List<Calendar>();
        }
        public IActionResult OnGet(string? type = null)
        {

            if (!User.Identity!.IsAuthenticated)
            {
                return RedirectToPage("/Account/Login", new { ReturnUrl = "/Appointment/Index", Area = "Identity" });
            }
            if (!User.IsInRole(SD.CLIENT_ROLE))
            {
                TempData["access_denied"] = "Access Denied. If you believe you should have access, report this to the administrator.";
                return RedirectToPage("../Index");
            }
            if(type == null)
            {
                return NotFound();
            }

            if (type.Equals("Tutoring"))
            {
                //Example of how to retrieve provider locations                                                 //This will be userId, not a hard-coded value
                var providerAvailabilities = _unitOfWork.ProviderAvailability.GetAll().Where(p => p.ProviderId == "f8749ce3-86b8-4173-9bcd-478717de9e0e")
                        .Select(l => l.LocationId).ToList();
                var locations = _unitOfWork.Location.GetAll().Where(l => providerAvailabilities.Contains(l.Id)).Select(x => new SelectListItem
                {
                    Text = x.LocationValue,
                    Value = x.Id.ToString()
                });

                var availabilities = _unitOfWork.ProviderAvailability.GetAll(includes: "Provider").Where(p => p.Provider.Title.Equals("Tutor"));
                foreach (var a in availabilities)
                {
                    CalendarObj.Add(new Calendar
                    {
                        Id = a.Id,
                        Name = _unitOfWork.AppUser.Get(x => x.Id == a.ProviderId).FullName,
                        StartTime = DateTimeParser.ParseDateTime(a.StartTime),
                        EndTime = DateTimeParser.ParseDateTime(a.EndTime)
                    });
                }
                
            }
            else if (type.Equals("NewStudent"))
            {
                var availabilities = _unitOfWork.ProviderAvailability.GetAll(includes: "Provider").Where(p => p.Provider.Title.Equals("Advisor") && p.Provider.AdvisementTypes.Split(',').Contains("NewStudent"));
                foreach (var a in availabilities)
                {
                    CalendarObj.Add(new Calendar
                    {
                        Id = a.Id,
                        Name = _unitOfWork.AppUser.Get(x => x.Id == a.ProviderId).FullName,
                        StartTime = DateTimeParser.ParseDateTime(a.StartTime),
                        EndTime = DateTimeParser.ParseDateTime(a.EndTime)
                    });
                }
            }
            else if (type.Equals("ExistingStudent"))
            {
                var availabilities = _unitOfWork.ProviderAvailability.GetAll(includes: "Provider").Where(p => p.Provider.Title.Equals("Advisor") && p.Provider.AdvisementTypes.Split(',').Contains("ExistingStudent"));
                foreach (var a in availabilities)
                {
                    CalendarObj.Add(new Calendar
                    {
                        Id = a.Id,
                        Name = _unitOfWork.AppUser.Get(x => x.Id == a.ProviderId).FullName,
                        StartTime = DateTimeParser.ParseDateTime(a.StartTime),
                        EndTime = DateTimeParser.ParseDateTime(a.EndTime)
                    });
                }
            }
            else if (type.Equals("FlexStudent"))
            {
                var availabilities = _unitOfWork.ProviderAvailability.GetAll(includes: "Provider").Where(p => p.Provider.Title.Equals("Advisor") && p.Provider.AdvisementTypes.Split(',').Contains("FlexStudent"));
                foreach (var a in availabilities)
                {
                    CalendarObj.Add(new Calendar
                    {
                        Id = a.Id,
                        Name = _unitOfWork.AppUser.Get(x => x.Id == a.ProviderId).FullName,
                        StartTime = DateTimeParser.ParseDateTime(a.StartTime),
                        EndTime = DateTimeParser.ParseDateTime(a.EndTime)
                    });
                }
            }
            else if (type.Equals("Instructing"))
            {
                var availabilities = _unitOfWork.ProviderAvailability.GetAll(includes: "Provider").Where(p => p.Provider.Title.Equals("Instructor"));
                foreach (var a in availabilities)
                {
                    CalendarObj.Add(new Calendar
                    {
                        Id = a.Id,
                        Name = _unitOfWork.AppUser.Get(x => x.Id == a.ProviderId).FullName,
                        StartTime = DateTimeParser.ParseDateTime(a.StartTime),
                        EndTime = DateTimeParser.ParseDateTime(a.EndTime)
                    });
                }
            }

            return Page();
        }

        public void OnPost()
        {
            string n = String.Format("{0}", !(Request.Form["availabilityId"].IsNullOrEmpty()) ? Request.Form["availabilityId"] : "");
        }
    }
}
