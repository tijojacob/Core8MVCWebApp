using Microsoft.AspNetCore.Mvc;
using Core8MVC.Models.Models;
using Core8MVCWebApp.Controllers.Data;

namespace Core8MVCWebApp.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _db;
        public CategoryController(ApplicationDbContext db)
        {
            _db= db;
        }
        public IActionResult Index()
        {
            List<Category> objCategoryList= _db.Categories.ToList();
            return View(objCategoryList);
        }
        public IActionResult CreateCategory()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateCategory(Category obj)
        {
            if (obj.Name == obj.DisplayOrder.ToString())
            {
                ModelState.AddModelError("Name", "Name and DisplayOrder cannot be the same");
            }
            else if (ModelState.IsValid)
            {
                _db.Categories.Add(obj);
                TempData["success"] = "Record created successfully";
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View();
            
        }

        public IActionResult EditCategory(int? Id)
        {
            if (Id==null || Id==0)
            {
                ModelState.AddModelError("", "Select a valid record");
                //return RedirectToAction("CreateCategory");
            }
            else if (Id>0)
            {
                Category? edit1 = _db.Categories.Find(Id);
                Category? edit2 = _db.Categories.FirstOrDefault(c => c.Id == Id);
                Category? edit3 = _db.Categories.Where(c => c.Id == Id).FirstOrDefault();

                if (edit2 == null)
                {
                    ModelState.AddModelError("", "Record not found");
                }
                else
                {
                    return View(edit2);
                }
            }
            return View();
        }
        [HttpPost]
        public IActionResult EditCategory(Category obj)
        {
            if (obj.Name == obj.DisplayOrder.ToString())
            {
                ModelState.AddModelError("Name", "Name and DisplayOrder cannot be the same");
            }
            else if (ModelState.IsValid)
            {
                _db.Categories.Update(obj);
                TempData["success"] = "Record updated successfully";
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View();

        }

        public IActionResult DeleteCategory(int? Id)
        {
            if (Id == null || Id == 0)
            {
                ModelState.AddModelError("", "Select a valid Record");
                //return RedirectToAction("CreateCategory");
            }
            else if (Id > 0)
            {
                Category? delete1 = _db.Categories.Find(Id);
                Category? delete2 = _db.Categories.FirstOrDefault(c => c.Id == Id);
                Category? delete3 = _db.Categories.Where(c => c.Id == Id).FirstOrDefault();

                if (delete2 == null)
                {
                    ModelState.AddModelError("", "Record not found");
                }
                else
                {
                    return View(delete2);
                }
            }
            return View();
        }
        [HttpPost,ActionName("DeleteCategory")]
        public IActionResult DeleteCategoryPOST(int? Id)
        {
            Category? delete1 = _db.Categories.Find(Id);
            Category? delete2 = _db.Categories.FirstOrDefault(c => c.Id == Id);
            Category? delete3 = _db.Categories.Where(c => c.Id == Id).FirstOrDefault();
            _db.Categories.Remove(delete2);
            TempData["success"] = "Record deleted successfully";
            _db.SaveChanges();
            return RedirectToAction("Index");            
        }

    }
}
