using DataAccess;
using Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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


        public async Task OnGet(string id)
        {
            AppUser = _unitOfWork.AppUser.Get(u => u.Id == id);
            var roles = await _userManager.GetRolesAsync(AppUser);
            UsersRoles = roles.ToList();
            OldRoles = roles.ToList();
            AllRoles = _roleManager.Roles.Select(r=> r.Name).ToList();
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
                    Provider providerEntry = new Provider();
                    providerEntry.AppUserId = AppUser.Id;
                }
            }
            await _unitOfWork.CommitAsync();
            return RedirectToPage("./Index", new { success = true, message = "Update Successful" });
        }
    }
}
