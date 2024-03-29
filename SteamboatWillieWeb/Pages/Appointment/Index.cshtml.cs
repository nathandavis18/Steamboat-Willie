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
        public Calendar[]? CalendarObj { get; set; }
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
        }
        public IActionResult OnGet(string? type = null)
        {
            CalendarObj = new Calendar[]
            {
                new Calendar {Id = "4", Name = "Dr. Fry", StartTime = DateTimeParser.ParseDateTime(DateTime.Parse("2024-03-28 13:30:00")), EndTime = DateTimeParser.ParseDateTime(DateTime.Parse("2024-03-28 14:00:00")) }, //Test data, needs to be pulled from DB
                new Calendar {Id = "18", Name = "Anderson", StartTime = "2024-03-29T09:30:00", EndTime = "2024-03-29T10:00:00"},
                new Calendar {Id = "27", Name = "Huson", StartTime = "2024-03-29T10:30:00", EndTime = "2024-03-29T11:00:00" }
            };
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


                //_unitOfWork.ProviderAvailability.GetAll().Where(p => p.Provider.Title.Equals("Tutor"));
            }
            else if (type.Equals("NewStudent"))
            {
                //_unitOfWork.ProviderAvailability.GetAll().Where(p => p.Provider.Title.Equals("Advisor") && p.Provider.AdvisementTypes.Split(',').Contains("NewStudent"));
                  //NewStudent,ExistingStudent,FlexStudent
            }
            else if (type.Equals("ExistingStudent"))
            {
                //_unitOfWork.ProviderAvailability.GetAll().Where(p => p.Provider.Title.Equals("Advisor") && p.Provider.AdvisementTypes.Split(',').Contains("ExistingStudent"));
            }
            else if (type.Equals("FlexStudent"))
            {
                //_unitOfWork.ProviderAvailability.GetAll().Where(p => p.Provider.Title.Equals("Advisor") && p.Provider.AdvisementTypes.Split(',').Contains("FlexStudent"));
            }
            else if (type.Equals("Instructing"))
            {
                //_unitOfWork.ProviderAvailability.GetAll().Where(p => p.Provider.Title.Equals("Instructor"));
            }

            return Page();
        }

        public void OnPost()
        {
            string n = String.Format("{0}", !(Request.Form["availabilityId"].IsNullOrEmpty()) ? Request.Form["availabilityId"] : "");
        }
    }
}
