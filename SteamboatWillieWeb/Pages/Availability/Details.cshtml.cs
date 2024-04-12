using DataAccess;
using Infrastructure.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Infrastructure.Models;
using Utility;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace SteamboatWillieWeb.Pages.Availability
{
    public class DetailsModel : PageModel
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;
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

        public DetailsModel(UnitOfWork unitOfWork, IEmailSender emailSender)
        {
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
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
                TempData["error"] = "Access Denied. If you believe you should have access, report this to the administrator.";
                return RedirectToPage("../Index");
            }
            providerAvailability = _unitOfWork.ProviderAvailability.GetById(id);
            if (id == null || providerAvailability == null)
            {
                TempData["error"] = "Access Denied. If you believe you should have access, report this to the administrator.";
                return RedirectToPage("../Index");
            }
            var clientName = "";
            var wnumber = "";
            var comments = "";
            var clientEmail = "";
            Appointment = _unitOfWork.Appointment.GetById(id);
            if (providerAvailability.Scheduled)
            {
                var client = _unitOfWork.AppUser.GetById(Appointment.ClientId);
                clientName = client.FullName;
                wnumber = client.WNumber;
                comments = Appointment.StudentComments;
                clientEmail = client.Email;
            }
            var date = providerAvailability.StartTime.ToString("MM/dd/yyyy");
            var time = providerAvailability.StartTime.ToString("h:mm tt") + " - " + providerAvailability.EndTime.ToString("h:mm tt");
            var location = _unitOfWork.Location.GetById(providerAvailability.LocationId).LocationValue;
            details = new Details { date = date, time = time, clientName = clientName, wnumber = wnumber, clientEmail = clientEmail, location = location, studentComments = comments };
            return Page();
        }

        public async Task<IActionResult> OnPost(string id, bool? removeAppointmentOnly, bool? removeEverything, bool? removeAvailability)
        {
            if (!ModelState.IsValid)
            {
                TempData["error"] = "Data Error unable to connect to DB";
                return Page();
            }
            else {
                providerAvailability = _unitOfWork.ProviderAvailability.GetById(id);
                Appointment = _unitOfWork.Appointment.GetById(id);
                if (removeAppointmentOnly.HasValue)
                {
                    var clientEmail = _unitOfWork.AppUser.GetById(Appointment.ClientId).Email;
                    var providerName = _unitOfWork.AppUser.GetById(providerAvailability.ProviderId).FullName;
                    await _emailSender.SendEmailAsync(clientEmail, "Appointment Canceled", EmailFormats.AppointmentCanceled.Replace("[ProviderName]", providerName));
                    _unitOfWork.Appointment.Delete(Appointment);
                    providerAvailability.Scheduled = false;
                    _unitOfWork.ProviderAvailability.Update(providerAvailability);
                    TempData["success"] = "Appointment Deleted Successfully";

                }
                else if (removeEverything.HasValue)
                {
                    var clientEmail = _unitOfWork.AppUser.GetById(Appointment.ClientId).Email;
                    var providerName = _unitOfWork.AppUser.GetById(providerAvailability.ProviderId).FullName;
                    await _emailSender.SendEmailAsync(clientEmail, "Appointment Canceled", EmailFormats.AppointmentCanceled.Replace("[ProviderName]", providerName));
                    _unitOfWork.Appointment.Delete(Appointment);
                    _unitOfWork.ProviderAvailability.Delete(providerAvailability);
                    TempData["success"] = "Appointment and Availability Deleted Successfully";
                }
                else if (removeAvailability.HasValue)
                {
                    _unitOfWork.ProviderAvailability.Delete(providerAvailability);
                    TempData["success"] = "Availability Deleted Successfully";
                }
                await _unitOfWork.CommitAsync();
                return RedirectToPage("/Index");
            }   
        }
    }
}
