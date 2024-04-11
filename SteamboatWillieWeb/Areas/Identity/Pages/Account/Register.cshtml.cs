// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.Rendering;
using Utility;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Configuration;
using DataAccess;

namespace SteamboatWillieWeb.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly IUserStore<AppUser> _userStore;
        private readonly IUserEmailStore<AppUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UnitOfWork _unitOfWork;

        public RegisterModel(
            UserManager<AppUser> userManager,
            IUserStore<AppUser> userStore,
            SignInManager<AppUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            RoleManager<IdentityRole> roleManager,
            UnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        [BindProperty]
        public ClientInputModel ClientInput {  get; set; }

        [BindProperty]
        public ProviderInputModel ProviderInput { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

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


            [Phone]
            [Display(Name = "Phone number")]
            [RegularExpression("\\([0-9]{3}\\) [0-9]{3}-[0-9]{4}", ErrorMessage = "Phone number must follow the format (xxx) xxx-xxxx")]
            [StringLength(15)]
            [Required]
            public string PhoneNumber { get; set; }
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Display(Name = "W#")]
            [RegularExpression("^W([0-9]{8}$)", ErrorMessage = "W# must match the format of W########")]
            [StringLength(9)]
            [Required]
            public string WNumber { get; set; }

            public string Role {  get; set; }
            public IEnumerable<SelectListItem> RoleList { get; set; }

            [Required]
            public string DepartmentId { get; set; }
            public IEnumerable<SelectListItem> Departments { get; set; }
        }

        public class ClientInputModel
        {
            [Required]
            [Display(Name = "Class Level")]
            public string ClassLevel { get; set; }

            [Required]
            [Display(Name = "Student Type")]
            public string StudentType { get; set; }

            [Display(Name = "Major")]
            public string DepartmentId { get; set; }

        }

        public class ProviderInputModel
        {
            [Required]
            [Display(Name = "Title")]
            public string Title { get; set; }

            [Display(Name = "Department")]
            public string DepartmentId { get; set; }

            public bool CreatingProvider { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null, bool creatingProvider = false)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            Input = new InputModel()
            {
                RoleList = _roleManager.Roles.Select(r => r.Name).Select(x => new SelectListItem()
                {
                    Text = x,
                    Value = x
                }),
                Departments = _unitOfWork.Department.GetAll().Select(x => new SelectListItem()
                {
                    Text = x.DepartmentName,
                    Value = x.Id.ToString()
                }),
                Role = "",
            };
            if (!creatingProvider)
            {
                ClientInput = new ClientInputModel();
            }
            else
            {
                ProviderInput = new ProviderInputModel()
                {
                    CreatingProvider = creatingProvider
                };
            }
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = CreateUser();
                user.FName = Input.FName;
                user.LName = Input.LName;
                user.PhoneNumber = Input.PhoneNumber;
                user.WNumber = Input.WNumber;
                user.ProfilePictureURL = "default.png";
                _unitOfWork.AppUser.Update(user);

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    var htmlMessage = EmailFormats.ConfirmEmail.Replace("[ConfirmEmailLink]", HtmlEncoder.Default.Encode(callbackUrl));

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                            $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    //We can change this so it doesn't automatically send the email confirmation

                    await _userManager.AddToRoleAsync(user, (Input.Role == null) ? SD.CLIENT_ROLE : Input.Role); //Adds user to Client Role by default,
                                                                                                                 //or adds them to the chosen role if selection was made by admin

                    if(await _userManager.IsInRoleAsync(user, SD.CLIENT_ROLE))
                    {
                        Client clientEntry = new Client();
                        clientEntry.AppUserId = userId;
                        clientEntry.DepartmentId = Int32.Parse(Input.DepartmentId);
                        clientEntry.ClassLevel = ClientInput.ClassLevel;
                        clientEntry.StudentType = ClientInput.StudentType;
                        _unitOfWork.Client.Add(clientEntry);
                    }
                    if(await _userManager.IsInRoleAsync(user, SD.PROVIDER_ROLE))
                    {
                        Provider providerEntry = new Provider();
                        providerEntry.AppUserId = userId;
                        providerEntry.DepartmentId = Int32.Parse(Input.DepartmentId);
                        providerEntry.Title = ProviderInput.Title;
                        providerEntry.AdvisementTypes = ",";
                        providerEntry.StartTime = DateTime.Parse("01/01/0001 08:00:00");
                        providerEntry.EndTime = DateTime.Parse("01/01/0001 20:00:00");
                        ProviderInput.CreatingProvider = true;
                        _unitOfWork.Provider.Add(providerEntry);
                    }
                    await _unitOfWork.CommitAsync();

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        if (!ProviderInput.CreatingProvider)
                        {
                            await _signInManager.SignInAsync(user, isPersistent: false);
                        }
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private AppUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<AppUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(AppUser)}'. " +
                    $"Ensure that '{nameof(AppUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<AppUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<AppUser>)_userStore;
        }
    }
}
