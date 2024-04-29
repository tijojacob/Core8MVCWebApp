using Core8MVCWebAppRazor.Data;
using Core8MVCWebAppRazor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Core8MVCWebAppRazor.Pages.Categories
{
    [BindProperties]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public Category objCategory { get; set; }
        public CreateModel(ApplicationDbContext db)
        {
            _db = db;
        }
        public void OnGet()
        {

        }

        public IActionResult OnPost()
        {
            _db.Categories.Add(objCategory);
            _db.SaveChanges();
            TempData["success"] = "Record created successfully";
            return RedirectToPage("Index");
        }
    }
}
