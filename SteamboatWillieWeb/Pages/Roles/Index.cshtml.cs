using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

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
        public string Message { get; set; }

        public async Task OnGetAsync(bool success = false, string message = null)
        {
            Success = success;
            Message = message;
            RolesObj = _roleManager.Roles;

        }
    }

}
