using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Utility;

namespace SteamboatWillieWeb.Pages.Roles
{
    public class UpsertModel : PageModel
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public UpsertModel(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        [BindProperty]
        public IdentityRole CurrentRole { get; set; }
        [BindProperty]
        public bool IsUpdate { get; set; }

        public async Task<IActionResult> OnGetAsync(string? id)
        {
            if (!User.Identity!.IsAuthenticated)
            {
                return RedirectToPage("/Account/Login", new { ReturnUrl = "~/Roles/Index", Area = "Identity" });
            }
            if (!User.IsInRole(SD.ADMIN_ROLE))
            {
                TempData["access_denied"] = "Access Denied. If you believe you should have access, report this to the administrator.";
                return RedirectToPage("../Index");
            }
            if (id != null)
            {
                CurrentRole = await _roleManager.FindByIdAsync(id);
                IsUpdate = true;
            }
            else
            {
                CurrentRole = new IdentityRole();
                IsUpdate = false;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {

            if (!IsUpdate)
            {
                CurrentRole.NormalizedName = CurrentRole.Name.ToUpper();

                await _roleManager.CreateAsync(CurrentRole);
                return RedirectToPage("./Index", new { success = true, message = "Successfully Added" });
            }
            else
            {
                CurrentRole.NormalizedName = CurrentRole.Name.ToUpper();

                await _roleManager.UpdateAsync(CurrentRole);
                return RedirectToPage("./Index", new { success = true, message = "Update Successful" });
            }

        }
    }

}
