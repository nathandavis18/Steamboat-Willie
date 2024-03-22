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
                DateOfBirth = user.DateOfBirth,
                WNumber = user.WNumber,
                Departments = departments.Select(x => new SelectListItem()
                {
                    Text = x.DepartmentName,
                    Value = x.Id.ToString()
                }),
                PhoneNumber = phoneNumber
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
                    Title = provider.Title
                };
            }
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
            user.DateOfBirth = Input.DateOfBirth;
            user.WNumber = Input.WNumber;
            var client = await _unitOfWork.Client.GetAsync(c => c.AppUserId == user.Id);
            if (client != null)
            {
                client.ClassLevel = ClientInput.ClassLevel;
                client.StudentType = ClientInput.StudentType;
                _unitOfWork.Client.Update(client);
            }

            var provider = await _unitOfWork.Provider.GetAsync(p => p.AppUserId == user.Id);
            if (provider != null)
            {
                provider.Title = ProviderInput.Title;
                _unitOfWork.Provider.Update(provider);
            }

            await _unitOfWork.CommitAsync();

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
