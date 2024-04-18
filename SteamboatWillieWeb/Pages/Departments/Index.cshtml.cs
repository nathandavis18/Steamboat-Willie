using DataAccess;
using Infrastructure.Models;
using Infrastructure.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Utility;

namespace SteamboatWillieWeb.Pages.Departments
{
    public class IndexModel : PageModel
    {
        private readonly UnitOfWork _unitOfWork;
        public PaginatedList<Department>? Departments { get; set; }

        public IndexModel(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult OnGet(int? pageIndex)
        {
            if (!User.Identity!.IsAuthenticated)
            {
                return RedirectToPage("/Account/Login", new { ReturnUrl = "/Availability/Index", Area = "Identity" });
            }
            if (!User.IsInRole(SD.ADMIN_ROLE))
            {
                TempData["error"] = "Access Denied. If you believe you should have access, report this to the administrator.";
                return RedirectToPage("../Index");
            }
            List<Department> departmentsList = _unitOfWork.Department.GetAll().OrderBy(dept => dept.DepartmentName).ToList();

            int pageSize = 10;
            Departments = PaginatedList<Department>.Create(departmentsList, pageIndex ?? 1, pageSize);

            return Page();
        }
        public IActionResult OnPost(int id, int pageNum)
        {
            var department = _unitOfWork.Department.GetById(id);
            if (department == null)
            {
                return RedirectToPage();
            }

            department.IsDisabled = !department.IsDisabled; //Inverts the result. If it is false, it sets it to true, and vice versa.
            _unitOfWork.Department.Update(department);
            _unitOfWork.Commit();
            return RedirectToPage("./Index", new {pageIndex = pageNum});
        }
    }
}
