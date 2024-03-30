using DataAccess;
using Infrastructure.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Infrastructure.Models;
using Utility;

namespace SteamboatWillieWeb.Pages.Availability
{
    public class DetailsModel : PageModel
    {
        private readonly UnitOfWork _unitOfWork;
        public Infrastructure.Models.Appointment Appointment { get; set; }
        public ProviderAvailability providerAvailability { get; set; }
        public Details details { get; set; }

        public class Details
        {
            public string date;
            public string time;
            public string clientName;
            public string wnumber;
            public string clientEmail;
            public string location;
            public string studentComments;
        }

        public DetailsModel(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            Appointment = new Infrastructure.Models.Appointment();
            providerAvailability = new ProviderAvailability();
        }
        public IActionResult OnGet(string id)
        {
            if (!User.Identity!.IsAuthenticated)
            {
                return RedirectToPage("/Account/Login", new { ReturnUrl = "~/Roles/Index", Area = "Identity" });
            }
            if (!User.IsInRole(SD.PROVIDER_ROLE))
            {
                TempData["access_denied"] = "Access Denied. If you believe you should have access, report this to the administrator.";
                return RedirectToPage("../Index");
            }
            providerAvailability = _unitOfWork.ProviderAvailability.GetAll().Where(x => x.Id == id).FirstOrDefault();
            if (id == null || providerAvailability == null)
            {
                TempData["access_denied"] = "Access Denied. If you believe you should have access, report this to the administrator.";
                return RedirectToPage("../Index");
            }
            var clientName = "";
            var wnumber = "";
            var comments = "";
            var clientEmail = "";
            Appointment = _unitOfWork.Appointment.GetAll().Where(x => x.ProviderAvailabilityId == id).FirstOrDefault();
            if (providerAvailability.Scheduled)
            {
                var client = _unitOfWork.AppUser.GetAll().Where(x => x.Id == Appointment.ClientId).FirstOrDefault();
                clientName = client.FullName;
                wnumber = client.WNumber;
                comments = Appointment.StudentComments;
                clientEmail = client.Email;
            }
            var date = providerAvailability.StartTime.ToString("MM/dd/yyyy");
            var time = providerAvailability.StartTime.ToString("h:mm tt") + " - " + providerAvailability.EndTime.ToString("h:mm tt");
            var location = _unitOfWork.Location.GetAll(x => x.Id == providerAvailability.LocationId).Select(x => x.LocationValue).FirstOrDefault();
            details = new Details { date = date, time = time, clientName = clientName, wnumber = wnumber, clientEmail = clientEmail, location = location, studentComments = comments };
            return Page();
        }

        public IActionResult OnPost(string id, bool? removeAppointmentOnly, bool? removeEverything, bool? removeAvailability)
        {
            if (!ModelState.IsValid)
            {
                TempData["error"] = "Data Error unable to connect to DB";
                return Page();
            }
            else {
                providerAvailability = _unitOfWork.ProviderAvailability.GetAll().Where(x => x.Id == id).FirstOrDefault();
                Appointment = _unitOfWork.Appointment.GetAll().Where(x => x.ProviderAvailabilityId == id).FirstOrDefault();
                if (removeAppointmentOnly.HasValue)
                {
                    _unitOfWork.Appointment.Delete(Appointment);
                    providerAvailability.Scheduled = false;
                    TempData["success"] = "Appointment Deleted Successfully";
                }
                else if (removeEverything.HasValue)
                {
                    _unitOfWork.Appointment.Delete(Appointment);
                    _unitOfWork.ProviderAvailability.Delete(providerAvailability);
                    TempData["success"] = "Appointment and Availability Deleted Successfully";
                }
                else if (removeAvailability.HasValue)
                {
                    _unitOfWork.ProviderAvailability.Delete(providerAvailability);
                    TempData["success"] = "Availability Deleted Successfully";
                }
                _unitOfWork.Commit();
                return RedirectToPage("/Index");
            }   
        }
    }
}
