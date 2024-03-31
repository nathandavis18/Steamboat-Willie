using DataAccess;
using Infrastructure.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Utility;

namespace SteamboatWillieWeb.Pages.Departments
{
    public class IndexModel : PageModel
    {
        private readonly UnitOfWork _unitOfWork;
        public PaginatedList<Department> Departments { get; set; }

        public IndexModel(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> OnGet(int? pageIndex)
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
            List<Department> departmentsList = _unitOfWork.Department.GetAll().ToList();

            int pageSize = 10;
            Departments = PaginatedList<Department>.Create(departmentsList, pageIndex ?? 1, pageSize);

            return Page();
        }
        public IActionResult OnPost(int id)
        {
            var department = _unitOfWork.Department.GetById(id);
            if (department == null)
            {
                return RedirectToPage();
            }

            _unitOfWork.Department.Delete(department);
            _unitOfWork.Commit();
            return RedirectToPage();
        }
    }
}
