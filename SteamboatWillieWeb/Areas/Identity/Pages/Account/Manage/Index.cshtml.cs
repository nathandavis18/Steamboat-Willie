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

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            /// [Required]
            [Required]
            [Display(Name = "First Name")]
            public string FName { get; set; }

            [Required]
            [Display(Name = "Last Name")]
            public string? LName { get; set; }

            [Required]
            [Display(Name = "Birthdate")]
            [DataType(DataType.Date)]
            public DateTime DateOfBirth { get; set; }

            [Display(Name = "W#")]
            [RegularExpression("^W([0-9]{8}$)", ErrorMessage = "W# must match the format of W########")]
            [StringLength(9)]
            public string WNumber {  get; set; }

            [Phone]
            [Display(Name = "Phone number")]
            [RegularExpression("\\([0-9]{3}\\) [0-9]{3}-[0-9]{4}", ErrorMessage = "Phone number must follow the format (xxx) xxx-xxxx")]
            [StringLength(15)]
            public string PhoneNumber { get; set; }

            [Display(Name = "Major")]
            public string MajorID { get; set; }

            [Display(Name = "Department")]
            public string DepartmentID {  get; set; }
            public IEnumerable<SelectListItem> Departments { get; set; }

            [Display(Name = "Title")]
            public string Title { get; set; }
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
                MajorID = (client != null ? client.DepartmentId.ToString() : null),
                FName = user.FName,
                LName = user.LName,
                DateOfBirth = user.DateOfBirth,
                WNumber = (client != null ? client.StudentId : null),
                Departments = departments.Select(x => new SelectListItem()
                {
                    Text = x.DepartmentName,
                    Value = x.Id.ToString()
                }),
                DepartmentID = (provider != null ? provider.DepartmentId.ToString() : null),
                Title = (provider != null ? provider.Title : null),
                PhoneNumber = phoneNumber
            };
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
            var client = await _unitOfWork.Client.GetAsync(c => c.AppUserId == user.Id);
            if (client != null)
            {
                client.StudentId = Input.WNumber;
                client.DepartmentId = Input.MajorID != null ? Int32.Parse(Input.MajorID) : null;
                _unitOfWork.Client.Update(client);
            }

            var provider = await _unitOfWork.Provider.GetAsync(p => p.AppUserId == user.Id);
            if (provider != null)
            {
                provider.Title = Input.Title;
                provider.DepartmentId = Input.DepartmentID != null ? Int32.Parse(Input.DepartmentID): null;
                _unitOfWork.Provider.Update(provider);
            }

            await _unitOfWork.CommitAsync();

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
