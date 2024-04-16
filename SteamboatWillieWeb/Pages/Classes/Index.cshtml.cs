using DataAccess;
using Infrastructure.Models;
using Infrastructure.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Utility;

namespace SteamboatWillieWeb.Pages.Classes
{
	public class IndexModel : PageModel
    {
        private readonly UnitOfWork _unitOfWork;
        public PaginatedList<Class>? Classes;

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
            List<Class> classList = _unitOfWork.Class.GetAll().OrderBy(clas => clas.Name).ToList();

            int pageSize = 10;
            Classes = PaginatedList<Class>.Create(classList, pageIndex ?? 1, pageSize);

            return Page();
        }
        public IActionResult OnPost(int id)
        {
            var clas = _unitOfWork.Class.GetById(id);
            if (clas == null)
            {
                return RedirectToPage();
            }

            _unitOfWork.Class.Delete(clas);
            _unitOfWork.Commit();
            return RedirectToPage();
        }
    }
}
