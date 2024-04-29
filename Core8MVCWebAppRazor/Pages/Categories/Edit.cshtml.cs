using Core8MVCWebAppRazor.Data;
using Core8MVCWebAppRazor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Core8MVCWebAppRazor.Pages.Categories
{
    [BindProperties]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public Category objCategory { get; set; }
        public EditModel(ApplicationDbContext db)
        {
            _db = db;
        }
        public void OnGet(int Id)
        {
            if (Id == null || Id == 0)
            {
                ModelState.AddModelError("", "Select a valid data");
                //return RedirectToAction("CreateCategory");
            }
            else if (Id > 0)
            {
                Category? edit1 = _db.Categories.Find(Id);
                Category? edit2 = _db.Categories.FirstOrDefault(c => c.Id == Id);
                Category? edit3 = _db.Categories.Where(c => c.Id == Id).FirstOrDefault();

                if (edit2 == null)
                {
                    ModelState.AddModelError("", "Data not found");
                }
                else
                {
                    objCategory = edit2;
                }
            }
            //objCategory = _db.Categories.FirstOrDefault(c => c.Id == Id);
        }

        public IActionResult OnPost()
        {
            _db.Categories.Update(objCategory);
            _db.SaveChanges();
            TempData["success"] = "Record updated successfully";
            return RedirectToPage("Index");
        }
    }
}
