using DataAccess;
using Infrastructure.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Utility;
using AppUser = Infrastructure.Models.AppUser;

namespace SteamboatWillieWeb.Pages.Users
{
    public class IndexModel : PageModel
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public IndexModel(UnitOfWork unit, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _unitOfWork = unit;
            _roleManager = roleManager;
        }

        public PaginatedList<AppUser>? ApplicationUsers { get; set; }
        public Dictionary<string, List<string>>? UserRoles { get; set; }
        public List<SelectListItem>? Roles {  get; set; }

        public string? CurrentSearch { get; set; }
        public string? CurrentRole { get; set; }
        public string? CurrentWSearch {  get; set; }
        public string? CurrentEmail {  get; set; }

        public async Task<IActionResult> OnGetAsync(int? pageIndex, string searchString, string roleSort, string wSearch, string emailSearch)
        {
            if (!User.Identity!.IsAuthenticated)
            {
                return RedirectToPage("/Account/Login", new { ReturnUrl = "~/Users/Index", Area = "Identity" });
            }
            if (!User.IsInRole(SD.ADMIN_ROLE))
            {
                TempData["error"] = "Access Denied. If you believe you should have access, report this to the administrator.";
                return RedirectToPage("../Index");
            }
            CurrentSearch = searchString;
            CurrentRole = roleSort;
            CurrentWSearch = wSearch;
            CurrentEmail = emailSearch;
            Roles = _roleManager.Roles.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Name
            }).ToList()!;

            UserRoles = new Dictionary<string, List<string>>();
            int pageSize = 10;
            var appUsers = (await _unitOfWork.AppUser.GetAllAsync()).ToList();
            if (!String.IsNullOrEmpty(searchString))
            {
                appUsers = appUsers.Where(a => a.FullName.ToUpper().Contains(searchString.ToUpper())).ToList();
            }
            if (!String.IsNullOrEmpty(roleSort))
            {
                appUsers = appUsers.Where(a => _userManager.GetRolesAsync(a).GetAwaiter().GetResult().Contains(roleSort)).ToList();
            }
            if(!String.IsNullOrEmpty(wSearch))
            {
                appUsers = appUsers.Where(a => !(String.IsNullOrEmpty(a.WNumber))).ToList();
                appUsers = appUsers.Where(a => a.WNumber!.Contains(wSearch.ToUpper())).ToList();
            }
            if (!String.IsNullOrEmpty(emailSearch))
            {
                appUsers = appUsers.Where(a => a.NormalizedEmail.Contains(emailSearch.ToUpper())).ToList();
            }
            appUsers = appUsers.OrderBy(x => x.LName).ThenBy(x => x.FName).ThenBy(x => x.WNumber).ToList();
            ApplicationUsers = PaginatedList<AppUser>.Create(appUsers, pageIndex ?? 1, pageSize);
            foreach (var user in ApplicationUsers)
            {
                var userRole = await _userManager.GetRolesAsync(user);
                UserRoles.Add(user.Id, userRole.ToList());
            }
            return Page();
        }

        public async Task<IActionResult> OnPostLockUnlock(string id)
        {
            var user = _unitOfWork.AppUser.GetById(id);
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
