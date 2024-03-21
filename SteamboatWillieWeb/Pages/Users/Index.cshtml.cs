using DataAccess;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Utility;
using AppUser = Infrastructure.Models.AppUser;

namespace SteamboatWillieWeb.Pages.Users
{
    public class IndexModel : PageModel
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;

        public IndexModel(UnitOfWork unit, UserManager<AppUser> userManager)
        {
            _userManager = userManager;
            _unitOfWork = unit;
        }

        public IEnumerable<AppUser> ApplicationUsers { get; set; }
        public Dictionary<string, List<string>> UserRoles { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!User.Identity!.IsAuthenticated)
            {
                return RedirectToPage("/Account/Login", new { ReturnUrl = "~/Users/Index", Area = "Identity" });
            }
            if (!User.IsInRole(SD.ADMIN_ROLE))
            {
                TempData["access_denied"] = "Access Denied. If you believe you should have access, report this to the administrator.";
                return RedirectToPage("../Index");
            }
            UserRoles = new Dictionary<string, List<string>>();
            ApplicationUsers = (IEnumerable<AppUser>)_unitOfWork.AppUser.GetAll();
            foreach (var user in ApplicationUsers)
            {
                var userRole = await _userManager.GetRolesAsync(user);
                UserRoles.Add(user.Id, userRole.ToList());
            }
            return Page();
        }

        public async Task<IActionResult> OnPostLockUnlock(string id)
        {
            var user = _unitOfWork.AppUser.Get(u => u.Id == id);
            if (user.LockoutEnd == null)
            {
                user.LockoutEnd = DateTime.Now.AddYears(150);
                user.LockoutEnabled = true;
            }
            else
            {
                user.LockoutEnd = null;
                user.LockoutEnabled = false;
            }
            _unitOfWork.AppUser.Update(user);
            await _unitOfWork.CommitAsync();
            return RedirectToPage();
        }
    }
}
