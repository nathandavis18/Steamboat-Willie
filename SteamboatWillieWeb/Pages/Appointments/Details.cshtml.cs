using DataAccess;
using Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SteamboatWillieWeb.Pages.Availability;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata.Ecma335;
using Utility;

namespace SteamboatWillieWeb.Pages.Appointments
{
    public class RegisterAppointmentModel : PageModel
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;

        public class AppointmentModel
        {
            public string AvailabilityId { get; set; }
            public string Date { get; set; }
            public string Time {  get; set; }
            public string Location { get; set; }
            public string ProviderType { get; set; }
            public string ProviderName {  get; set; }
            [Display(Name = "Comments")]
            public string Comments { get; set; }

            public string CancelReturn {  get; set; }

            public bool IsScheduled {  get; set; }
        }

        public RegisterAppointmentModel(UnitOfWork unitOfWork, UserManager<AppUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public ProviderAvailability? SelectedAvailability { get; set; }

        [BindProperty]
        public AppointmentModel AppointmentInput { get; set; }


        public IActionResult OnGet(string id)
        {
            if (!User.Identity!.IsAuthenticated)
            {
                return RedirectToPage("/Account/Login", new { ReturnUrl = "~/Index", Area = "Identity" });
            }
            if (!User.IsInRole(SD.CLIENT_ROLE))
            {
                TempData["access_denied"] = "Access Denied. If you believe you should have access, report this to the administrator.";
                return RedirectToPage("../Index");
            }

            var userId = _userManager.GetUserId(User);
            var client = _unitOfWork.Client.Get(c => c.AppUserId == userId);
            SelectedAvailability = _unitOfWork.ProviderAvailability.Get(p => p.Id == id);
            AppointmentInput = new AppointmentModel
            {
                AvailabilityId = SelectedAvailability.Id,
                Date = SelectedAvailability.StartTime.ToLongDateString(),
                Time = SelectedAvailability.StartTime.ToShortTimeString() + " - " + SelectedAvailability.EndTime.ToShortTimeString(),
                Location = _unitOfWork.Location.GetById(SelectedAvailability.LocationId).LocationValue,
                ProviderType = _unitOfWork.Provider.Get(p => p.AppUserId == SelectedAvailability.ProviderId).Title,
                ProviderName = _unitOfWork.AppUser.Get(p => p.Id == SelectedAvailability.ProviderId).FullName,
                IsScheduled = SelectedAvailability.Scheduled
            };

            var appointment = _unitOfWork.Appointment.Get(a => a.ProviderAvailabilityId == AppointmentInput.AvailabilityId);
            if(appointment != null)
            {
                AppointmentInput.Comments = appointment.StudentComments;
            }

            switch (AppointmentInput.ProviderType)
            {
                case "Advisor":
                    switch (client.ClassLevel)
                    {
                        case "Freshman":
                            AppointmentInput.CancelReturn = "NewStudent";
                            break;
                        default:
                            AppointmentInput.CancelReturn = "ExistingStudent";
                            break;
                    }
                    switch (client.StudentType)
                    {
                        case "FlexStudent":
                            AppointmentInput.CancelReturn = "FlexStudent";
                            break;
                        default:
                            break;
                    }
                    break;
                case "Instructor":
                    AppointmentInput.CancelReturn = "Instructing";
                    break;
                case "Tutor":
                    AppointmentInput.CancelReturn = "Tutoring";
                    break;
                default:
                    break;
            }

            return Page();
        }

        public async Task<IActionResult> OnPost(bool cancel = false, bool update = false)
        {
            var user = await _userManager.GetUserAsync(User);
            var availability = _unitOfWork.ProviderAvailability.Get(p => p.Id == AppointmentInput.AvailabilityId);

            if (cancel)
            {
                var app = _unitOfWork.Appointment.Get(a => a.ProviderAvailabilityId == AppointmentInput.AvailabilityId);
                _unitOfWork.Appointment.Delete(app);
                
                availability.Scheduled = false;
                _unitOfWork.ProviderAvailability.Update(availability);

                await _unitOfWork.CommitAsync();
                return RedirectToPage("../Index");
            }

            if (update)
            {
                var app = _unitOfWork.Appointment.Get(a => a.ProviderAvailabilityId == AppointmentInput.AvailabilityId);
                app.StudentComments = AppointmentInput.Comments;
                _unitOfWork.Appointment.Update(app);

                await _unitOfWork.CommitAsync();
                return RedirectToPage("../Index");
            }


            var appointment = new Appointment
            {
                ProviderAvailabilityId = AppointmentInput.AvailabilityId,
                ClientId = user.Id,
                Description = AppointmentInput.ProviderType + " Appointment",
                StudentComments = AppointmentInput.Comments
            };

            _unitOfWork.Appointment.Add(appointment);

            availability.Scheduled = true;
            _unitOfWork.ProviderAvailability.Update(availability);

            await _unitOfWork.CommitAsync();

            return RedirectToPage("../Index");
        }
    }
}
