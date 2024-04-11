using DataAccess;
using Infrastructure.Models;
using Infrastructure.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.IdentityModel.Tokens;
using Utility;

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
        }

        private readonly UnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        public IndexModel(UnitOfWork unitOfWork, UserManager<AppUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            CalendarObj = new List<Calendar>();
        }

        [BindProperty]
        public AppointmentViewModel? InputModel { get; set; }

        public IActionResult OnGet(string? type = null)
        {
            if (!User.Identity!.IsAuthenticated)
            {
                return RedirectToPage("/Account/Login", new { ReturnUrl = "/Appointment/Index", Area = "Identity" });
            }
            var user = _userManager.GetUserAsync(User).GetAwaiter().GetResult();
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
                var availabilities = _unitOfWork.ProviderAvailability.GetAll(includes: "Provider").Where(p => p.Provider.Title.Equals("Tutor") && !p.Scheduled && !(p.Provider.AppUserId == user.Id) && p.StartTime >= DateTime.Now);
                foreach (var a in availabilities)
                {
                    CalendarObj.Add(new Calendar
                    {
                        Id = a.Id,
                        Name = _unitOfWork.AppUser.GetById(a.ProviderId).FullName,
                        StartTime = DateTimeParser.ParseDateTime(a.StartTime),
                        EndTime = DateTimeParser.ParseDateTime(a.EndTime)
                    });
                }
                
            }
            else if (type.Equals("NewStudent"))
            {
                var availabilities = _unitOfWork.ProviderAvailability.GetAll(includes: "Provider").Where(p => p.Provider.Title.Equals("Advisor") && p.Provider.AdvisementTypes.Split(',').Contains("NewStudent") && !p.Scheduled && p.StartTime >= DateTime.Now);
                foreach (var a in availabilities)
                {
                    CalendarObj.Add(new Calendar
                    {
                        Id = a.Id,
                        Name = _unitOfWork.AppUser.GetById(a.ProviderId).FullName,
                        StartTime = DateTimeParser.ParseDateTime(a.StartTime),
                        EndTime = DateTimeParser.ParseDateTime(a.EndTime)
                    });
                }
            }
            else if (type.Equals("ExistingStudent"))
            {
                var availabilities = _unitOfWork.ProviderAvailability.GetAll(includes: "Provider").Where(p => p.Provider.Title.Equals("Advisor") && p.Provider.AdvisementTypes.Split(',').Contains("ExistingStudent") && !p.Scheduled && p.StartTime >= DateTime.Now);
                foreach (var a in availabilities)
                {
                    CalendarObj.Add(new Calendar
                    {
                        Id = a.Id,
                        Name = _unitOfWork.AppUser.GetById(a.ProviderId).FullName,
                        StartTime = DateTimeParser.ParseDateTime(a.StartTime),
                        EndTime = DateTimeParser.ParseDateTime(a.EndTime)
                    });
                }
            }
            else if (type.Equals("FlexStudent"))
            {
                var availabilities = _unitOfWork.ProviderAvailability.GetAll(includes: "Provider").Where(p => p.Provider.Title.Equals("Advisor") && p.Provider.AdvisementTypes.Split(',').Contains("FlexStudent") && !p.Scheduled && p.StartTime >= DateTime.Now);
                foreach (var a in availabilities)
                {
                    CalendarObj.Add(new Calendar
                    {
                        Id = a.Id,
                        Name = _unitOfWork.AppUser.GetById(a.ProviderId).FullName,
                        StartTime = DateTimeParser.ParseDateTime(a.StartTime),
                        EndTime = DateTimeParser.ParseDateTime(a.EndTime)
                    });
                }
            }
            else if (type.Equals("Instructing"))
            {
                var availabilities = _unitOfWork.ProviderAvailability.GetAll(includes: "Provider").Where(p => p.Provider.Title.Equals("Instructor") && !p.Scheduled && p.StartTime >= DateTime.Now);
                foreach (var a in availabilities)
                {
                    CalendarObj.Add(new Calendar
                    {
                        Id = a.Id,
                        Name = _unitOfWork.AppUser.GetById(a.ProviderId).FullName,
                        StartTime = DateTimeParser.ParseDateTime(a.StartTime),
                        EndTime = DateTimeParser.ParseDateTime(a.EndTime)
                    });
                }
            }

            

            return Page();
        }

        public IActionResult OnPost()
        {
            string id = Request.Form["availabilityId"];

            return RedirectToPage("./Details", new { id = id } );
        }


        public PartialViewResult OnGetRegisterAppointment(string? id)
        {
            var availability = _unitOfWork.ProviderAvailability.GetById(id);
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

        public IActionResult OnPostRegisterAppointment(string id)
        {
            var userId = _userManager.GetUserId(User);
            var availability = _unitOfWork.ProviderAvailability.GetById(id);
            availability.Scheduled = true;
            _unitOfWork.ProviderAvailability.Update(availability);

            Appointment appointment = new Appointment
            {
                ProviderAvailabilityId = id,
                ClientId = userId,
                StudentComments = InputModel.Comments,
                Description = "Appointment",
                StudentNoShow = false
            };

            _unitOfWork.Appointment.Add(appointment);
            _unitOfWork.Commit();

            return RedirectToPage("../Index");
        }
    }
}
