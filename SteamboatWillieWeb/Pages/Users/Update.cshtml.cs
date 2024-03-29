using DataAccess;
using Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Utility;

namespace SteamboatWillieWeb.Pages.Users
{
    public class UpdateModel : PageModel
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public UpdateModel(UnitOfWork unit, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _unitOfWork = unit;
            _roleManager = roleManager;
        }

        [BindProperty]
        public AppUser AppUser { get; set; }
        public List<string> UsersRoles { get; set; }
        public List<string> AllRoles { get; set; }
        public List<string> OldRoles { get; set; }


        public async Task<IActionResult> OnGetAsync(string id)
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
            AppUser = _unitOfWork.AppUser.Get(u => u.Id == id);
            var roles = await _userManager.GetRolesAsync(AppUser);
            UsersRoles = roles.ToList();
            OldRoles = roles.ToList();
            AllRoles = _roleManager.Roles.Select(r=> r.Name).ToList();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var newRoles = Request.Form["roles"];
            UsersRoles = newRoles.ToList();
            var oldRoles = await _userManager.GetRolesAsync(AppUser);
            var rolesToAdd = new List<string>();
            var user = _unitOfWork.AppUser.Get(u=>u.Id == AppUser.Id);

            user.FName = AppUser.FName;
            user.LName = AppUser.LName;
            user.Email = AppUser.Email;
            _unitOfWork.AppUser.Update(user);
            _unitOfWork.Commit();

            foreach (var role in UsersRoles) {
                if (!oldRoles.Contains(role))
                {
                    rolesToAdd.Add(role);
                }
            }
            foreach (var role in oldRoles)
            {
                if (!UsersRoles.Contains(role))
                {
                    var result = await _userManager.RemoveFromRoleAsync(user , role);
                }
            }
            var result1 = await _userManager.AddToRolesAsync(user, rolesToAdd.AsEnumerable());

            //Creating/Removing Client and Provider DB Entires as needed
            if (!(await _userManager.IsInRoleAsync(AppUser, SD.CLIENT_ROLE)))
            {
                var client = await _unitOfWork.Client.GetAsync(c => c.AppUserId == AppUser.Id);
                if (client != null)
                {
                    _unitOfWork.Client.Delete(client);
                }
            }
            else if (await _userManager.IsInRoleAsync(AppUser, SD.CLIENT_ROLE))
            {
                var client = await _unitOfWork.Client.GetAsync(c => c.AppUserId == AppUser.Id);
                if (client == null)
                {
                    Client clientEntry = new Client();
                    clientEntry.AppUserId = AppUser.Id;
                    clientEntry.DepartmentId = 1;
                    clientEntry.ClassLevel = "Freshman";
                    clientEntry.StudentType = "Full-Time Student";
                    _unitOfWork.Client.Add(clientEntry);
                }
            }
            if (!(await _userManager.IsInRoleAsync(AppUser, SD.PROVIDER_ROLE)))
            {
                var provider = await _unitOfWork.Provider.GetAsync(p => p.AppUserId == AppUser.Id);
                if(provider != null)
                {
                    _unitOfWork.Provider.Delete(provider);
                }
            }
            else if (await _userManager.IsInRoleAsync(AppUser, SD.PROVIDER_ROLE))
            {
                var provider = await _unitOfWork.Provider.GetAsync(p => p.AppUserId == AppUser.Id);
                if (provider == null)
                {
                    Client? client;
                    Provider providerEntry = new Provider();
                    providerEntry.AppUserId = AppUser.Id;
                    client = await _unitOfWork.Client.GetAsync(c => c.AppUserId == AppUser.Id);
                    if (client != null)
                    {
                        providerEntry.DepartmentId = client.DepartmentId;
                        providerEntry.Title = "Tutor";

                    }
                    else
                    {
                        providerEntry.DepartmentId = 1;
                        providerEntry.Title = "Instructor";
                    }
                    providerEntry.AdvisementTypes = ",";
                    providerEntry.StartTime = DateTime.Parse("01/01/0001 08:00:00");
                    providerEntry.EndTime = DateTime.Parse("01/01/0001 18:00:00");
                    _unitOfWork.Provider.Add(providerEntry);
                }
            }
            await _unitOfWork.CommitAsync();
            return RedirectToPage("./Index", new { success = true, message = "Update Successful" });
        }
    }
}
