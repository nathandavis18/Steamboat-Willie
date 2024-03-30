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
                var availabilities = _unitOfWork.ProviderAvailability.GetAll(includes: "Provider").Where(p => p.Provider.Title.Equals("Tutor") && !p.Scheduled);
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
                var availabilities = _unitOfWork.ProviderAvailability.GetAll(includes: "Provider").Where(p => p.Provider.Title.Equals("Advisor") && p.Provider.AdvisementTypes.Split(',').Contains("NewStudent") && !p.Scheduled);
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
                var availabilities = _unitOfWork.ProviderAvailability.GetAll(includes: "Provider").Where(p => p.Provider.Title.Equals("Advisor") && p.Provider.AdvisementTypes.Split(',').Contains("ExistingStudent") && !p.Scheduled);
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
                var availabilities = _unitOfWork.ProviderAvailability.GetAll(includes: "Provider").Where(p => p.Provider.Title.Equals("Advisor") && p.Provider.AdvisementTypes.Split(',').Contains("FlexStudent") && !p.Scheduled);
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
                var availabilities = _unitOfWork.ProviderAvailability.GetAll(includes: "Provider").Where(p => p.Provider.Title.Equals("Instructor") && !p.Scheduled);
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
            string n = Request.Form["availabilityId"];
        }
    }
}
