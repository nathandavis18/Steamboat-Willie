// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using DataAccess;
using Infrastructure.Models;
using Infrastructure.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Utility;
using Utility.GoogleCalendar;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SteamboatWillieWeb.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        public string? CurrentUserTitle;

        [BindProperty]
        public IEnumerable<SelectListItem>? Classes { get; set; }

        [BindProperty]
        public IEnumerable<SelectListItem>? ProviderClasses { get; set; }

        public IndexModel(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            UnitOfWork unitOfWork,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public ProfileInputModel Input { get; set; }

        [BindProperty]
        public WeberStudentInputModel WeberStudentInput { get; set; }

        [BindProperty]
        public ProviderInputModel ProviderInput { get; set; }

        [BindProperty]
        public FileInputModel FileInput { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>

        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        public class FileInputModel
        {
            [DataType(DataType.Upload)]

            public IFormFile ImgFile { get; set; }
        }


        private string ReturnUrl { get; set; }

        private async Task LoadAsync(AppUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            var client = await _unitOfWork.Client.GetAsync(c => c.AppUserId == user.Id);
            var provider = await _unitOfWork.Provider.GetAsync(p => p.AppUserId == user.Id);

            var departments = _unitOfWork.Department.GetAll();

            Username = userName;

            Input = new ProfileInputModel
            {
                FName = user.FName,
                LName = user.LName,
                WNumber = user.WNumber,
                PhoneNumber = phoneNumber,
                ProfilePictureURL = user.ProfilePictureURL,
                IsIntegrated = user.GoogleCalendarIntegration.Value
            };
            if (await _userManager.IsInRoleAsync(user, SD.CLIENT_ROLE))
            {
                WeberStudentInput = new WeberStudentInputModel()
                {
                    IsWeberStudent = client.IsWeberStudent,
                    Departments = _unitOfWork.Department.GetAll().Where(d => d.IsDisabled != true).OrderBy(x => x.DepartmentName).Select(x => new SelectListItem
                    {
                        Text = x.DepartmentName,
                        Value = x.Id.ToString()
                    })
                };
                if (client.IsWeberStudent)
                {
                    WeberStudentInput.DepartmentId = client.DepartmentId.ToString();
                    WeberStudentInput.ClassLevel = client.ClassLevel;
                    WeberStudentInput.StudentType = client.StudentType;
                }
            }
            if (await _userManager.IsInRoleAsync(user, SD.PROVIDER_ROLE))
            {
                ProviderInput = new ProviderInputModel()
                {
                    Departments = _unitOfWork.Department.GetAll().Where(d => d.IsDisabled != true).OrderBy(x => x.DepartmentName).Select(x => new SelectListItem
                    {
                        Text = x.DepartmentName,
                        Value = x.Id.ToString()
                    }),
                    DepartmentId = provider.DepartmentId.ToString(),
                    Title = provider.Title,
                    StartTime = provider.StartTime,
                    EndTime = provider.EndTime,
                    NewStudent = provider.AdvisementTypes.Split(",").Contains("NewStudent"),
                    ExistingStudent = provider.AdvisementTypes.Split(",").Contains("ExistingStudent"),
                    FlexStudent = provider.AdvisementTypes.Split(",").Contains("FlexStudent"),
                    Color = provider.HexColor
                };
            }
            FileInput = new FileInputModel();
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var currentUserId = _userManager.GetUserId(User);
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("../Login", new { ReturnUrl = "~/Identity/Account/Manage/Index" });
            }

            if(await _userManager.IsInRoleAsync(user, SD.PROVIDER_ROLE))
            {
                var provider = _unitOfWork.Provider.Get(p => p.AppUserId == currentUserId);
                CurrentUserTitle = provider.Title;

                var classes = _unitOfWork.Class
                    .GetAll()
                    .Distinct()
                    .ToList();

                Classes = _unitOfWork.Class
                    .GetAll()
                    .Where(c => c.IsDisabled != true)
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                    .ToList();

                var providerClasses = _unitOfWork.ProviderClass
                    .GetAll()
                    .Where(pc => pc.ProviderId == user.Id)
                    .Select(pc => pc.ClassId)
                    .ToList();

                ProviderClasses = _unitOfWork.ProviderClass
                    .GetAll()
                    .Where(p => providerClasses.Contains(p.ClassId))
                    .Select(p => new SelectListItem { Value = p.ClassId.ToString(), Text = p.ClassId.ToString() })
                    .ToList();
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    TempData["error"] = "There was a problem setting your phone number. Please try again.";
                    return RedirectToPage();
                }
            }
            user.FName = Input.FName;
            user.LName = Input.LName;
            user.WNumber = Input.WNumber;

            var client = await _unitOfWork.Client.GetAsync(c => c.AppUserId == user.Id);
            if (client != null)
            {
                client.IsWeberStudent = WeberStudentInput.IsWeberStudent;
                if (WeberStudentInput.IsWeberStudent)
                {
                    client.ClassLevel = WeberStudentInput.ClassLevel;
                    client.StudentType = WeberStudentInput.StudentType;
                    client.DepartmentId = int.Parse(WeberStudentInput.DepartmentId);
                }
                _unitOfWork.Client.Update(client);
            }

            var provider = await _unitOfWork.Provider.GetAsync(p => p.AppUserId == user.Id);
            if (provider != null)
            {

                provider.Title = ProviderInput.Title;
                provider.StartTime = ProviderInput.StartTime;
                provider.EndTime = ProviderInput.EndTime;
                provider.AdvisementTypes = ",";
                if (ProviderInput.NewStudent)
                {
                    provider.AdvisementTypes += "NewStudent,";
                }
                if (ProviderInput.ExistingStudent)
                {
                    provider.AdvisementTypes += "ExistingStudent,";
                }
                if (ProviderInput.FlexStudent)
                {
                    provider.AdvisementTypes += "FlexStudent,";
                }
                if(ProviderInput.Title != "Advisor")
                {
                    provider.AdvisementTypes = ",";
                }
                provider.DepartmentId = ProviderInput.DepartmentId != null ?
                       int.Parse(ProviderInput.DepartmentId) :
                       int.Parse(WeberStudentInput.DepartmentId);
                provider.HexColor = ProviderInput.Color;
                _unitOfWork.Provider.Update(provider);

                foreach (var classId in Request.Form["Classes"])
                {
                    var providerClasses = _unitOfWork.ProviderClass.GetAll().Where(pc => pc.ProviderId == user.Id);
                    int id = int.Parse(classId);
                    bool isChecked = Request.Form["Classes"].Contains(classId);
                    var existingProviderClasses = _unitOfWork.ProviderClass.GetAll().Where(pc => pc.ProviderId == user.Id);
                    bool exists = existingProviderClasses.Any(pc => pc.ClassId == id);
                    if (isChecked && !exists)
                    {
                        var newProviderClass = new ProviderClass
                        {
                            ProviderId = user.Id,
                            ClassId = id
                        };
                        _unitOfWork.ProviderClass.Add(newProviderClass);
                    }
                    var classesToRemove = providerClasses.Where(pc => !Request.Form["Classes"].Contains(pc.ClassId.ToString()));
                    foreach (var classToRemove in classesToRemove)
                    {
                        _unitOfWork.ProviderClass.Delete(classToRemove);
                    }
                }




                
            }

            await _unitOfWork.CommitAsync();

            await _signInManager.RefreshSignInAsync(user);
            TempData["success"] = "Your profile has been updated!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostImageAsync()
        {
            var imageBase64 = Request.Form["blob"].ToString();
            var startIndex = imageBase64.LastIndexOf("base64,") + 7;
            var base64String = imageBase64.Substring(startIndex, imageBase64.Length - startIndex);
            var imgContents = WebEncoders.Base64UrlDecode(base64String);

            Image finalImage = Image.Load<Rgba32>(imgContents);

            var user = await _userManager.GetUserAsync(User);
            var fileName = user.Id.Substring(0, 10) + "pfpImage.png";
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/profileimages", fileName);
            await finalImage.SaveAsync(path);

            user.ProfilePictureURL = fileName;
            _unitOfWork.AppUser.Update(user);
            await _unitOfWork.CommitAsync();

            TempData["success"] = "Profile picture updated successfully!";

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnPostGoogleCalendarIntegrationAsync(bool integrate)
        {
            var user = await _userManager.GetUserAsync(User);
            if (integrate)
            {
                var credential = await ValidateUser.ValidateUserCalendar(user.Id, _configuration);
                user.GoogleCalendarIntegration = ValidateUser.IsUserValidated(credential);
                if (user.GoogleCalendarIntegration == true)
                {
                    TempData["success"] = "You have successfully integrated with Google Calendar!";
                }
                else
                {
                    TempData["error"] = "Something went wrong. Please try again!";
                }
            }
            else
            {
                user.GoogleCalendarIntegration = false;
                TempData["warning"] = "Your appointments will no longer automatically integrate with your Google Calendar.";
            }

            _unitOfWork.AppUser.Update(user);
            await _unitOfWork.CommitAsync();

            return RedirectToPage("./Index");
        }
    }
}
