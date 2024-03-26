using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json.Converters;
using System.ComponentModel;
using System.Dynamic;

namespace SteamboatWillieWeb.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        public Calendar[] CalendarObj {  get; set; }
        public class Calendar //Test class, needs to be updated
        {
            public string Id { get; set; }
            public string Title { get; set; }
            public string Date { get; set; }
        }

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            CalendarObj = new Calendar[]
            {
                new Calendar {Id = "3", Title = "Test", Date = "2024-03-25" }, //Test data, needs to be pulled from DB
                new Calendar {Id = "4", Title = "Test2", Date = "2024-03-26"},
                new Calendar {Id = "5", Title = "Test3", Date = "2024-03-26" }
            };
            return Page();
        }
    }
}
