// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using DataAccess;
using Infrastructure.Models;
using Infrastructure.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Utility;

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
        public RegisterInputModel Input { get; set; }

        [BindProperty]
        public WeberStudentInputModel WeberStudentInput {  get; set; }

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


        public async Task OnGetAsync(string returnUrl = null, bool creatingProvider = false, bool isWeberStudent = false)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            Input = new RegisterInputModel()
            {
                RoleList = _roleManager.Roles.Select(r => r.Name).Select(x => new SelectListItem()
                {
                    Text = x,
                    Value = x
                }),
                Role = "",
                CreatingProvider = creatingProvider,
            };
            if (!creatingProvider)
            {
                WeberStudentInput = new WeberStudentInputModel()
                {
                    Departments = _unitOfWork.Department.GetAll().Where(d => d.IsDisabled != true).OrderBy(x => x.DepartmentName).Select(x => new SelectListItem
                    {
                        Text = x.DepartmentName,
                        Value = x.Id.ToString()
                    }),
                    IsWeberStudent = isWeberStudent
                };
            }
            else
            {
                ProviderInput = new ProviderInputModel()
                {
                    Departments = _unitOfWork.Department.GetAll().Where(d => d.IsDisabled != true).OrderBy(x => x.DepartmentName).Select(x => new SelectListItem
                    {
                        Text = x.DepartmentName,
                        Value = x.Id.ToString()
                    }),
                };
            }

            if (!User.IsInRole(SD.ADMIN_ROLE))
            {
                Input.CreatingProvider = false;
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
                user.ProfilePictureURL = "default.png";
                _unitOfWork.AppUser.Update(user);

                user.WNumber = Input.WNumber;

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");
                    var userId = await _userManager.GetUserIdAsync(user);

                    if (!Input.CreatingProvider)
                    {
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        var callbackUrl = Url.Page(
                            "/Account/ConfirmEmail",
                            pageHandler: null,
                            values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                            protocol: Request.Scheme);

                        var htmlMessage = EmailFormats.ConfirmEmail.Replace("[ConfirmEmailLink]", HtmlEncoder.Default.Encode(callbackUrl));

                        await _emailSender.SendEmailAsync(Input.Email, "Confirm your email", htmlMessage);
                    }

                    await _userManager.AddToRoleAsync(user, (Input.Role == null) ? SD.CLIENT_ROLE : Input.Role); //Adds user to Client Role by default,
                                                                                                                 //or adds them to the chosen role if selection was made by admin

                    if (await _userManager.IsInRoleAsync(user, SD.CLIENT_ROLE))
                    {
                        Client clientEntry = new Client();
                        clientEntry.AppUserId = userId;
                        clientEntry.IsWeberStudent = WeberStudentInput.IsWeberStudent;
                        if (WeberStudentInput.IsWeberStudent)
                        {
                            clientEntry.DepartmentId = Int32.Parse(WeberStudentInput.DepartmentId);
                            clientEntry.ClassLevel = WeberStudentInput.ClassLevel;
                            clientEntry.StudentType = WeberStudentInput.StudentType;
                        }
                        _unitOfWork.Client.Add(clientEntry);
                    }
                    if(await _userManager.IsInRoleAsync(user, SD.PROVIDER_ROLE))
                    {
                        Provider providerEntry = new Provider();
                        providerEntry.AppUserId = userId;
                        providerEntry.DepartmentId = Int32.Parse(ProviderInput.DepartmentId);
                        providerEntry.Title = ProviderInput.Title;
                        providerEntry.AdvisementTypes = ",";
                        providerEntry.StartTime = TimeSpan.Parse("08:00:00");
                        providerEntry.EndTime = TimeSpan.Parse("20:00:00");
                        Input.CreatingProvider = true;
                        _unitOfWork.Provider.Add(providerEntry);

                        user.EmailConfirmed = true;

                        var callbackLink = Url.Page("/Account/Login", pageHandler: null, values: new { area = "Identity" }, protocol: Request.Scheme);
                        var message = EmailFormats.ProviderAccountCreated.Replace("[Email]", Input.Email).Replace("[Password]", Input.Password).Replace("[Link]", HtmlEncoder.Default.Encode(callbackLink));

                        await _emailSender.SendEmailAsync(Input.Email, "Account Created!", message);
                        _unitOfWork.AppUser.Update(user); //Provider Accounts don't need to verify their email.
                    }
                    await _unitOfWork.CommitAsync();

                    if (!Input.CreatingProvider && _userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        if (!Input.CreatingProvider)
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
