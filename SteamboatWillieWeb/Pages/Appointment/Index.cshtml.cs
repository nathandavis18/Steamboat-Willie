using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Utility;

namespace SteamboatWillieWeb.Pages.Appointment
{
    public class IndexModel : PageModel
    {
        public Calendar[] CalendarObj { get; set; }
        public class Calendar //Test class, needs to be updated
        {
            public string Id { get; set; }
            public string Title { get; set; }
            public string Date { get; set; }
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
                new Calendar {Id = "3", Title = "Test", Date = "2024-03-25" }, //Test data, needs to be pulled from DB
                new Calendar {Id = "4", Title = "Test2", Date = "2024-03-26"},
                new Calendar {Id = "5", Title = "Test3", Date = "2024-03-26" }
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
            string n = String.Format("{0}", Request.Form["availabilityId"]);
        }
    }
}
