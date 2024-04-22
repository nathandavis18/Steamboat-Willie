using DataAccess;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Drawing.Printing;
using System.Drawing;
using Utility;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using System.Diagnostics;
using System.ComponentModel;
using System.Text.RegularExpressions;
using Azure;
using SelectPdf;

namespace SteamboatWillieWeb.Pages.Availability
{

    public class Report
    {
        public int numAvailabilities { get; set; }
        public int numAppointments { get; set; }
        public string providerName { get; set; }
        public string dateRange { get; set; }
        public int numNoShows { get; set; }
        public double AppointmentAvailabilityPercent { get; set; }
        public double AppointmentNoShowPercent { get; set; }
    }


    public class ReportsModel : PageModel
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;

        [BindProperty]
        public ReportsInput ReportsModelInput { get; set; }

        public class ReportsInput
        {
            [Required]
            public string ProviderId { get; set; }

            [Required]
            [Display(Name = "Start Date")]
            [DataType(DataType.Date)]
            [DateValidator(ErrorMessage = "The Start Date must be before today")]
            public DateTime StartDate { get; set; }
            [Display(Name = "End Date")]
            [DataType(DataType.Date)]
            [DateValidator(ErrorMessage = "End Date must be the same as or after Start Date and before today")]
            public DateTime EndDate { get; set; }
            [Required]
            [Display(Name = "File Name")]
            public string FileName { get; set; }
        }



        public bool ContainsDisallowedCharacters(string fileName)
        {
            string pattern = @"[\\@/:*?""\[\]<>|.]";
            Regex regex = new Regex(pattern);
            return regex.IsMatch(fileName);
        }




        public ReportsModel(UnitOfWork unitOfWork, UserManager<AppUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }
        public IActionResult OnGet()
        {
            if (!User.Identity!.IsAuthenticated)
            {
                return RedirectToPage("/Account/Login", new { ReturnUrl = "/Availability/Index", Area = "Identity" });
            }
            if (!User.IsInRole(SD.PROVIDER_ROLE))
            {
                TempData["error"] = "Access Denied. If you believe you should have access, report this to the administrator.";
                return RedirectToPage("../Index");
            }
            var user = _userManager.GetUserAsync(User).GetAwaiter().GetResult();
            DateTime startDate = DateTime.Now.AddDays(-1);
            DateTime endDate = DateTime.Now.AddDays(-1);
            ReportsModelInput = new ReportsInput()
            {
                StartDate = startDate,
                EndDate = endDate,
                ProviderId = user.Id
            };
            return Page();
        }

        public IActionResult OnPost()
        {
            if (ReportsModelInput.StartDate.Date >= DateTime.Now.Date)
            {
                ModelState.AddModelError("ReportsModelInput.StartDate", "Start Date must be before today");
            }
            else if (ReportsModelInput.StartDate.Date < DateTime.Now.Date)
            {
                ModelState.Remove("ReportsModelInput.StartDate");
            }
            if (ReportsModelInput.EndDate.Date < ReportsModelInput.StartDate.Date || ReportsModelInput.EndDate.Date >= DateTime.Now.Date)
            {
                ModelState.AddModelError("ReportsModelInput.EndDate", "End Date must be the same as or after Start Date and before today");
            }
            else if (ReportsModelInput.EndDate.Date >= ReportsModelInput.StartDate.Date && ReportsModelInput.EndDate.Date < DateTime.Now.Date)
            {
                ModelState.Remove("ReportsModelInput.EndDate");
            }
            if (ReportsModelInput.FileName == null || ContainsDisallowedCharacters(ReportsModelInput.FileName)) 
            {
                ModelState.AddModelError("ReportsModelInput.FileName", "File Name must not contain any of the following: '\\\\@/:*?\"[]<>|.' example: MyReport");
            }
            else if (ReportsModelInput.FileName.Length > 0 && !ContainsDisallowedCharacters(ReportsModelInput.FileName))
            {
                ModelState.Remove("ReportsModelInput.FileName");
            }
            if (!ModelState.IsValid)
            {
                return Page();
            }

            HtmlToPdf report = new HtmlToPdf();
            var user = _userManager.GetUserAsync(User).GetAwaiter().GetResult();
            var providerName = user.FullName;
            IEnumerable<ProviderAvailability> providerAvailabilities = _unitOfWork.ProviderAvailability.GetAll().Where(x => x.ProviderId == user.Id && x.StartTime.Date >= ReportsModelInput.StartDate.Date && x.StartTime.Date <= ReportsModelInput.EndDate.Date);
            var numAvailabilities = providerAvailabilities.Count();
            List<Appointment> appointments = new List<Appointment>();
            foreach (ProviderAvailability availability in providerAvailabilities)
            {
                if (availability.Scheduled)
                {
                    appointments.Add(_unitOfWork.Appointment.GetAll().Where(x => x.ProviderAvailabilityId == availability.Id).FirstOrDefault());
                }
            }

            var numAppointments = appointments.Count;
            var AppointmentAvailabilityPercent = 0.0;
            if (numAvailabilities > 0)
            {
                if (numAppointments > 0)
                    AppointmentAvailabilityPercent = Math.Round(((double)numAppointments / numAvailabilities) * 100);
                else AppointmentAvailabilityPercent = 0;
            }
            else
            {
                AppointmentAvailabilityPercent = -1;
            }

            int no = 0;
            foreach (Appointment appointment in appointments)
            {
                if (appointment.StudentNoShow)
                {
                    no++;
                }
            }
            var numNoShows = no;
            var AppointmentNoShowPercent = 0.0;
            if (numAppointments > 0)
            {
                if (numNoShows > 0)
                    AppointmentNoShowPercent = Math.Round(((double)numNoShows / numAppointments) * 100);
                else AppointmentNoShowPercent = 0;
            }
            else
            {
                AppointmentNoShowPercent = -1;
            }
            var dateRange = "y";
            if (ReportsModelInput.StartDate == ReportsModelInput.EndDate)
            {
                dateRange = ReportsModelInput.StartDate.ToString("MM/dd/yyyy");
            }
            else
            {
                dateRange = ReportsModelInput.StartDate.ToString("MM/dd/yyyy") + " - " + ReportsModelInput.EndDate.ToString("MM/dd/yyyy");
            }

            string fileName = ReportsModelInput.FileName + ".pdf";
            
            report.Options.PdfPageSize = PdfPageSize.A4;
            report.Options.MarginBottom = 30;
            report.Options.MarginLeft = 30;
            report.Options.MarginRight = 30;
            report.Options.MarginTop = 30;

            string doozy = "<html>" +
                "<head>" +
                "<style>" +
                "td, th { font-size: 25px; text-align: left; } table { width: 100%; border-collapse: collapse; } tr { border-bottom: 1px solid black; } .one { font-size: 25px; width: 40%; }" +
                "footer { position: fixed; bottom: 0; width: 100%; color: #888; text-align: center; padding: 10px 0;" +
                "</style>" +
                "</head>" +
                "<body>" +
                "<p style=\"font-size:40px; font-weight: 900;\">Appointment Report for " + providerName + "</p>" +
                "<p style=\"font-size:30px;font-weight: 400;\">" + dateRange + "</p>" +
                "<hr style=\"height:10px;background-color:black\">" +
                "<table class=\"table\">" +
                "<thead><tr style=\"border-bottom: 3px solid black;\">" +
                "<th class=\"one\">Name</th><th>Number</th><th>Ratio</th><th>Percent</th></tr></thead>" +
                "<tbody>" +
                "<tr><td class=\"one\">Number of Availabilities</td><td>" + numAvailabilities + "</td><td>N/A</td><td>N/A</td></tr>";
            string appointmentrow = "<tr><td class=\"one\">Number of Appointments</td><td>" + numAppointments + "</td><td>" + numAppointments + "/" + numAvailabilities + "</td><td>";
            if (AppointmentAvailabilityPercent == -1) {
                appointmentrow += "N/A</td></tr>";
            }
            else
            {
                appointmentrow += AppointmentAvailabilityPercent + "%</td></tr>";
            }
            doozy += appointmentrow;
            string noshowrow = "<tr><td class=\"one\">Number of No Shows</td><td>" + numNoShows + "</td><td>" + numNoShows + "/" + numAppointments + "</td><td>";
            if (AppointmentNoShowPercent == -1)
            {
                noshowrow += "N/A</td></tr>";
            }
            else
            {
                noshowrow += AppointmentNoShowPercent + "%</td></tr>";
            }
            doozy += noshowrow +
            "</tbody>" +
                "</table>" +
                "<div style=\"padding-top: 1150px; text-align: center; color: gray; font-size: 20px;\">" +
                "Steamboat Scheduler Reports" +
                "</div>" +
                "</body>" +
                "</html>";

            PdfDocument doc = report.ConvertHtmlString(doozy);
            MemoryStream memoryStream = new MemoryStream();
            doc.Save(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);
            doc.Close();

            return File(memoryStream, "application/pdf", fileName);
        }
    }

}



