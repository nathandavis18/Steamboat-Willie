using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using DataAccess;
using Infrastructure.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Utility;

namespace SteamboatWillieWeb.Pages.Classes
{
	public class UpsertModel : PageModel
    {
        private readonly UnitOfWork _unitOfWork;
        public UpsertModel(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public string? ReturnUrl { get; set; }

        [BindProperty]
        public ClassModel ClassModelInput { get; set; }

        public class ClassModel
        {
            public string? Id { get; set; } = "0";

            [Required]
            [Display(Name = "Class")]
            [RegularExpression(@"^\d{4}$", ErrorMessage = "Class must be exactly four digits.")]
            [StringLength(4, MinimumLength = 4, ErrorMessage = "Class must be exactly four digits.")]
            public string? ClassName { get; set; }

            [Required]
            [Display(Name = "Department")]
            public string? DepartmentId { get; set; }
            public IEnumerable<SelectListItem>? Departments { get; set; }
        }


        public IActionResult OnGet(int? id = null, string? returnUrl = null)
        {
            if (!User.Identity!.IsAuthenticated)
            {
                return RedirectToPage("/Account/Login", new { ReturnUrl = "/Availability/Index", Area = "Identity" });
            }
            if (!User.IsInRole(SD.ADMIN_ROLE))
            {
                TempData["error"] = "Access Denied. If you believe you should have access, report this to the administrator.";
                return RedirectToPage("../Index");
            }

            ClassModelInput = new ClassModel()
            {
                Departments = _unitOfWork.Department
                    .GetAll()
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.DepartmentName })

            };

            if (id != null)
            {
                var cls = _unitOfWork.Class.GetById(id);
                var split = cls.Name.Split(" ");
                var dept = split[0];
                var classNum = split[1];
                ClassModelInput.Id = cls.Id.ToString();
                ClassModelInput.DepartmentId = _unitOfWork.Department.Get(d => d.DepartmentName.Equals(dept)).Id.ToString();
                ClassModelInput.ClassName = classNum.ToString();
            }

            ReturnUrl = returnUrl;
            return Page();
        }

        public IActionResult OnPost(string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                ClassModelInput.Departments = _unitOfWork.Department
                    .GetAll()
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.DepartmentName });

                return Page();
            }

            var departmentName = _unitOfWork.Department.GetById(int.Parse(ClassModelInput.DepartmentId)).DepartmentName;
            if (int.Parse(ClassModelInput.Id) != 0)
            {
                var classs = _unitOfWork.Class.GetById(int.Parse(ClassModelInput.Id));
                classs.Name = departmentName + " " + ClassModelInput.ClassName;
                _unitOfWork.Class.Update(classs);
            }
            else
            {
                var classs = new Class
                {
                    Name = departmentName + " " + ClassModelInput.ClassName,
                    IsDisabled = false
                };
                _unitOfWork.Class.Add(classs);
            }
            _unitOfWork.Commit();
            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            return RedirectToPage("./Index");
        }
    }
}
