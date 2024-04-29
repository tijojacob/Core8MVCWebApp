using Core8MVCWebAppRazor.Data;
using Core8MVCWebAppRazor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Core8MVCWebAppRazor.Pages.Categories
{
    [BindProperties]
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public Category objCategory { get; set; }
        public DeleteModel(ApplicationDbContext db)
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
                Category? delete1 = _db.Categories.Find(Id);
                Category? delete2 = _db.Categories.FirstOrDefault(c => c.Id == Id);
                Category? delete3 = _db.Categories.Where(c => c.Id == Id).FirstOrDefault();

                if (delete2 == null)
                {
                    ModelState.AddModelError("", "Data not found");
                }
                else
                {
                    objCategory = delete2;
                }
            }
            //objCategory = _db.Categories.FirstOrDefault(c => c.Id == Id);
        }
        public IActionResult OnPost()
        {
            _db.Categories.Remove(objCategory);
            _db.SaveChanges();
            TempData["success"] = "Record deleted successfully";
            return RedirectToPage("Index");
        }
    }
}
