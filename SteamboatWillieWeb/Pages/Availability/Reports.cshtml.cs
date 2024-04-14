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
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using System.Diagnostics;
using System.ComponentModel;
using System.Text.RegularExpressions;

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

            Report report = new Report();
            var user = _userManager.GetUserAsync(User).GetAwaiter().GetResult();
            report.providerName = user.FullName;
            IEnumerable<ProviderAvailability> providerAvailabilities = _unitOfWork.ProviderAvailability.GetAll().Where(x => x.ProviderId == user.Id && x.StartTime.Date >= ReportsModelInput.StartDate.Date && x.StartTime.Date <= ReportsModelInput.EndDate.Date); 
            report.numAvailabilities = providerAvailabilities.Count();
            List<Appointment> appointments = new List<Appointment>();
            foreach (ProviderAvailability availability in providerAvailabilities)
            {
                if (availability.Scheduled)
                {
                    appointments.Add(_unitOfWork.Appointment.GetAll().Where(x => x.ProviderAvailabilityId == availability.Id).FirstOrDefault());
                }
            }
            
            report.numAppointments = appointments.Count;
            if (report.numAvailabilities > 0)
            {
                if (report.numAppointments > 0)
                    report.AppointmentAvailabilityPercent = Math.Round(((double)report.numAppointments / report.numAvailabilities) * 100);
                else report.AppointmentAvailabilityPercent = 0;
            }
            else
            {
                report.AppointmentAvailabilityPercent = -1;
            }

            int no = 0;
            foreach (Appointment appointment in appointments)
            {
                if (appointment.StudentNoShow)
                {
                    no++;
                }
            }
            report.numNoShows = no;
            if (report.numAppointments > 0)
            {
                if (report.numNoShows > 0)
                    report.AppointmentNoShowPercent = Math.Round(((double)report.numNoShows / report.numAppointments) * 100);
                else report.AppointmentNoShowPercent = 0;
            }
            else
            {
                report.AppointmentNoShowPercent = -1;
            }

            if (ReportsModelInput.StartDate == ReportsModelInput.EndDate)
            {
                report.dateRange = ReportsModelInput.StartDate.ToString("MM/dd/yyyy");
            }
            else
            {
                report.dateRange = ReportsModelInput.StartDate.ToString("MM/dd/yyyy") + " - " + ReportsModelInput.EndDate.ToString("MM/dd/yyyy");
            }

            string fileName = ReportsModelInput.FileName + ".pdf";

            string homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string downloadsFolder = Path.Combine(homeDirectory, "Downloads");
            string filePath = Path.Combine(downloadsFolder, fileName);

            byte[] fileBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);

                    page.Header().Column(column =>
                    {
                        column.Item().Text($"Appointment Report for {report.providerName}").FontSize(24).Bold();

                        column.Item().Text($"{report.dateRange}").FontSize(16);
                        column.Item().PaddingVertical(20).LineHorizontal(5).LineColor(Colors.Black);
                    });
                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });
                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("Name");
                            header.Cell().Element(CellStyle).Text("Number");
                            header.Cell().Element(CellStyle).Text("Ratio");
                            header.Cell().Element(CellStyle).Text("Percent");
                            static QuestPDF.Infrastructure.IContainer CellStyle(QuestPDF.Infrastructure.IContainer container)
                            {
                                return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                            }
                        });
                        table.Cell().Element(CellStyle).Text("Number of Availabilities");
                        table.Cell().Element(CellStyle).Text($"{report.numAvailabilities}");
                        table.Cell().Element(CellStyle).Text("N/A");
                        table.Cell().Element(CellStyle).Text("N/A");


                        table.Cell().Element(CellStyle).Text("Number of Appointments");
                        table.Cell().Element(CellStyle).Text($"{report.numAppointments}");
                        table.Cell().Element(CellStyle).Text($"{report.numAppointments}/{report.numAvailabilities}");
                        if (report.AppointmentAvailabilityPercent == -1)
                        {
                            table.Cell().Element(CellStyle).Text("N/A");
                        }
                        else
                        {
                            table.Cell().Element(CellStyle).Text($"{report.AppointmentAvailabilityPercent}%");
                        }


                        table.Cell().Element(CellStyle).Text("Number of No Shows");
                        table.Cell().Element(CellStyle).Text($"{report.numNoShows}");
                        table.Cell().Element(CellStyle).Text($"{report.numNoShows}/{report.numAppointments}");
                        if (report.AppointmentNoShowPercent == -1)
                        {
                            table.Cell().Element(CellStyle).Text("N/A");
                        }
                        else
                        {
                            table.Cell().Element(CellStyle).Text($"{report.AppointmentNoShowPercent}%");
                        }


                        static QuestPDF.Infrastructure.IContainer CellStyle(QuestPDF.Infrastructure.IContainer container)
                        {
                            return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                        }

                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.DefaultTextStyle(x => x.Size(12));
                        text.Span("Steamboat Scheduler");
                    });
                });
            }).GeneratePdf();
            
            return File(fileBytes, "application/pdf", fileName);
        }
    }

}



