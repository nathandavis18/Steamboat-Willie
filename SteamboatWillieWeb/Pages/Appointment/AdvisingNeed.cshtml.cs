using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Utility;

namespace SteamboatWillieWeb.Pages.Appointment
{
    public class AdvisingNeedModel : PageModel
    {
       public IActionResult OnGet()
        {
            if (!User.Identity!.IsAuthenticated)
            {
                return RedirectToPage("/Account/Login", new { ReturnUrl = "/Appointment/AdvisingNeed", Area = "Identity" });
            }
            if (!User.IsInRole(SD.CLIENT_ROLE))
            {
                TempData["access_denied"] = "Access Denied. If you believe you should have access, report this to the administrator.";
                return RedirectToPage("../Index");
            }
            return Page();
        }
    }
}
