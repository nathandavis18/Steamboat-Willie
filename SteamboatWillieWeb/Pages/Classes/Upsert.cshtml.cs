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
        public Class Class { get; set; }

        [BindProperty]
        public ClassModel ClassModelInput { get; set; }

        public class ClassModel
        { 
            [BindProperty]
            [Display(Name = "Class")]
            [RegularExpression(@"^\d{4}$", ErrorMessage = "Class must be exactly four digits.")]
            public string? ClassName { get; set; }
        }

        [BindProperty]
        [Display(Name = "Department")]
        public Department department { get; set; }

        public IEnumerable<SelectListItem> Departments; 

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

            ClassModelInput = new ClassModel();

            var departments = _unitOfWork.Department
                .GetAll()
                .Distinct()
                .ToList();

            Departments = _unitOfWork.Department
                .GetAll()
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.DepartmentName })
                .ToList();

            if (id != null)
            {
                Class = _unitOfWork.Class.GetById(id);
                var split = Class.Name.Split(" ");
                var dept = split[0];
                var classNum = split[1];
                ClassModelInput.ClassName = classNum.ToString();
            }
            else
            {
                Class = new Class();
                Class.Id = 0;
            }

            ReturnUrl = returnUrl;
            return Page();
        }

        public IActionResult OnPost(string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                var departments = _unitOfWork.Department
                .GetAll()
                .Distinct()
                .ToList();

                Departments = _unitOfWork.Department
                    .GetAll()
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.DepartmentName })
                    .ToList();

                return Page();
            }

            var departmentName = _unitOfWork.Department.GetById(int.Parse(department.DepartmentName)).DepartmentName;
            if (Class.Id != 0)
            {
                var classs = _unitOfWork.Class.GetById(Class.Id);
                classs.Name = departmentName + " " + ClassModelInput.ClassName;
                _unitOfWork.Class.Update(classs);
            }
            else
            {
                var classs = new Class
                {
                    Name = departmentName + " " + ClassModelInput.ClassName
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
