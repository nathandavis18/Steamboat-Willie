using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Utility;

namespace SteamboatWillieWeb.Pages.Roles
{
    public class IndexModel : PageModel
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public IndexModel(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public IEnumerable<IdentityRole> RolesObj { get; set; }
        public bool Success { get; set; }
        public string? Message { get; set; }

        public IActionResult OnGet(bool success = false, string? message = null) //Does this need to be async?
         {
            if (!User.Identity!.IsAuthenticated)
            {
                return RedirectToPage("/Account/Login", new { ReturnUrl = "~/Roles/Index", Area = "Identity"});
            }
            if (!User.IsInRole(SD.ADMIN_ROLE))
            {
                TempData["access_denied"] = "Access Denied. If you believe you should have access, report this to the administrator.";
                return RedirectToPage("../Index");
            }
            Success = success;
            Message = message;
            RolesObj = _roleManager.Roles;
            return Page();
        }
    }

}
