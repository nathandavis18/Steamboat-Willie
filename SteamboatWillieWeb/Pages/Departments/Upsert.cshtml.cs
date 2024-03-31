using DataAccess;
using Infrastructure.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Utility;

namespace SteamboatWillieWeb.Pages.Departments
{
    public class UpsertModel : PageModel
    {
        private readonly UnitOfWork _unitOfWork;
        public UpsertModel(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public string? ReturnUrl { get; set; }

        [BindProperty]
        public Department Department { get; set; }

        public IActionResult OnGet(int? id = null, string? returnUrl = null)
        {
            if (!User.Identity!.IsAuthenticated)
            {
                return RedirectToPage("/Account/Login", new { ReturnUrl = "/Availability/Index", Area = "Identity" });
            }
            if (!User.IsInRole(SD.ADMIN_ROLE))
            {
                TempData["access_denied"] = "Access Denied. If you believe you should have access, report this to the administrator.";
                return RedirectToPage("../Index");
            }

            if(id != null)
            {
                Department = _unitOfWork.Department.GetById(id);
            }
            else
            {
                Department = new Department();
                Department.Id = 0;
            }
            ReturnUrl = returnUrl;
            return Page();
        }

        public IActionResult OnPost(string? returnUrl = null)
        {
            if(Department.Id != 0)
            {
                var department = _unitOfWork.Department.GetById(Department.Id);
                department.DepartmentName = Department.DepartmentName;
                _unitOfWork.Department.Update(department);
            }
            else
            {
                var department = new Department
                {
                    DepartmentName = Department.DepartmentName
                };
                _unitOfWork.Department.Add(department);
            }
            _unitOfWork.Commit();
            if(returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            return RedirectToPage("./Index");
        }
    }
}
