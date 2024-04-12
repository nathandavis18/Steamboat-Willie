// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using DataAccess;
using Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp;
using Utility;

namespace SteamboatWillieWeb.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UnitOfWork _unitOfWork;

        public IndexModel(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            UnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _unitOfWork = unitOfWork;
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
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        [BindProperty]
        public ClientInputModel ClientInput { get; set; }

        [BindProperty]
        public ProviderInputModel ProviderInput { get; set; }

        [BindProperty]
        public FileInputModel FileInput { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            [Required]
            [Display(Name = "First Name")]
            public string FName { get; set; }

            [Required]
            [DisplayName("Last Name")]
            public string LName { get; set; }

            [Required]
            [DisplayName("Birthdate")]
            [DataType(DataType.Date)]
            public DateTime DateOfBirth { get; set; }

            [Phone]
            [Display(Name = "Phone number")]
            [RegularExpression("\\([0-9]{3}\\) [0-9]{3}-[0-9]{4}", ErrorMessage = "Phone number must follow the format (xxx) xxx-xxxx")]
            [StringLength(15)]
            [Required]
            public string PhoneNumber { get; set; }

            [Display(Name = "W#")]
            [RegularExpression("^W([0-9]{8}$)", ErrorMessage = "W# must match the format of W########")]
            [StringLength(9)]
            [Required]
            public string WNumber { get; set; }

            [Required]
            public string DepartmentId { get; set; }
            public IEnumerable<SelectListItem> Departments { get; set; }

            [DisplayName("Profile Picture")]
            public string ProfilePictureURL { get; set; }
        }

        public class ClientInputModel
        {
            [Required]
            [Display(Name = "Class Level")]
            public string ClassLevel { get; set; }

            [Display(Name = "Major")]
            public string DepartmentId { get; set; }

            [Required]
            [Display(Name = "Student Type")]
            public string StudentType { get; set; }
        }

        public class ProviderInputModel
        {
            [Required]
            [Display(Name = "Title")]
            public string Title { get; set; }

            [Display(Name = "Department")]
            public string DepartmentId { get; set; }

            [Display(Name = "Advisement Types")]
            public string AdvisementTypes {  get; set; }

            [Display(Name = "New Student")]
            public bool NewStudent { get; set; }
            [Display(Name = "Current Student")]
            public bool ExistingStudent {  get; set; }
            [Display(Name = "Flex Student")]
            public bool FlexStudent { get; set; }

            [Display(Name = "Working Start Time")]
            [DataType(DataType.Time)]
            public DateTime StartTime { get; set; }

            [Display(Name = "Working End Time")]
            [DataType(DataType.Time)]
            public DateTime EndTime { get; set; }

            [Display(Name = "Your Color")]
            public string Color { get; set; }
        }

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

            Input = new InputModel
            {
                FName = user.FName,
                LName = user.LName,
                WNumber = user.WNumber,
                Departments = departments.Select(x => new SelectListItem()
                {
                    Text = x.DepartmentName,
                    Value = x.Id.ToString()
                }),
                PhoneNumber = phoneNumber,
                ProfilePictureURL = user.ProfilePictureURL
            };
            if (await _userManager.IsInRoleAsync(user, SD.CLIENT_ROLE))
            {
                Input.DepartmentId = client.DepartmentId.ToString();
                ClientInput = new ClientInputModel()
                {
                    ClassLevel = client.ClassLevel,
                    StudentType = client.StudentType
                };
            }
            if (await _userManager.IsInRoleAsync(user, SD.PROVIDER_ROLE))
            {
                Input.DepartmentId = provider.DepartmentId.ToString();
                ProviderInput = new ProviderInputModel()
                {
                    Title = provider.Title,
                    StartTime = provider.StartTime.Value,
                    EndTime = provider.EndTime.Value,
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
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("../Login", new { ReturnUrl = "~/Identity/Account/Manage/Index" });
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
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }
            user.FName = Input.FName;
            user.LName = Input.LName;
            user.WNumber = Input.WNumber;
            var client = await _unitOfWork.Client.GetAsync(c => c.AppUserId == user.Id);
            if (client != null)
            {
                client.ClassLevel = ClientInput.ClassLevel;
                client.StudentType = ClientInput.StudentType;
                client.DepartmentId = int.Parse(Input.DepartmentId);
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
                provider.DepartmentId = int.Parse(Input.DepartmentId);
                provider.HexColor = ProviderInput.Color;
                _unitOfWork.Provider.Update(provider);
            }

            await _unitOfWork.CommitAsync();

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostImageAsync()
        {
            if (FileInput.ImgFile == null)
            {
                TempData["error"] = "Error: No Image Selected";
                return RedirectToPage();
            }
            var extension = Path.GetExtension(FileInput.ImgFile.FileName);
            if(!(extension.Equals(".png") || extension.Equals(".jpg") || extension.Equals(".jpeg")))
            {
                TempData["error"] = "Error: Image file type not supported.";
                return RedirectToPage();
            }
            var user = await _userManager.GetUserAsync(User);
            var fileName = user.Id.Substring(0, 10) + user.FName.Substring(0, 3) + user.LName.Substring(0, 1) + "pfpImage.png";
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/profileimages", fileName);
            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write))
            {
                user.ProfilePictureURL = fileName;
                await FileInput.ImgFile.CopyToAsync(fs);
                _unitOfWork.AppUser.Update(user);
                await _unitOfWork.CommitAsync();
            }
            return RedirectToPage();
        }
    }
}
