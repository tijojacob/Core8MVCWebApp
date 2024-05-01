using Microsoft.AspNetCore.Mvc;
using Core8MVC.Models.Models;
using Core8MVCWebApp.Controllers.Data;
using Core8MVC.DataAccess.Repository.IRepository;

namespace Core8MVCWebApp.Areas.Admin.Controllers
{
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ProductController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            List<Product> objProductList = _unitOfWork._productRepository.GetAll().ToList();
            return View(objProductList);
        }
        public IActionResult CreateProduct()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateProduct(Product obj)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork._productRepository.Add(obj);
                TempData["success"] = "Record created successfully";
                _unitOfWork.Save();
                return RedirectToAction("Index");
            }
            return View();

        }

        public IActionResult EditProduct(int? Id)
        {
            if (Id == null || Id == 0)
            {
                ModelState.AddModelError("", "Select a valid record");
                //return RedirectToAction("CreateProduct");
            }
            else if (Id > 0)
            {
                Product? edit2 = _unitOfWork._productRepository.Get(c => c.Id == Id);

                //Product? edit1 = _db.Categories.Find(Id);
                //Product? edit2 = _db.Categories.FirstOrDefault(c => c.Id == Id);
                //Product? edit3 = _db.Categories.Where(c => c.Id == Id).FirstOrDefault();

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
        public IActionResult EditProduct(Product obj)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork._productRepository.Update(obj);
                TempData["success"] = "Record updated successfully";
                _unitOfWork.Save();
                return RedirectToAction("Index");
            }
            return View();

        }

        public IActionResult DeleteProduct(int? Id)
        {
            if (Id == null || Id == 0)
            {
                ModelState.AddModelError("", "Select a valid Record");
                //return RedirectToAction("CreateProduct");
            }
            else if (Id > 0)
            {
                Product? delete2 = _unitOfWork._productRepository.Get(c => c.Id == Id);


                //Product? delete1 = _db.Categories.Find(Id);
                //Product? delete2 = _db.Categories.FirstOrDefault(c => c.Id == Id);
                //Product? delete3 = _db.Categories.Where(c => c.Id == Id).FirstOrDefault();

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
        [HttpPost, ActionName("DeleteProduct")]
        public IActionResult DeleteProductPOST(int? Id)
        {
            Product? delete2 = _unitOfWork._productRepository.Get(c => c.Id == Id);


            //Product? delete1 = _db.Categories.Find(Id);
            //Product? delete2 = _db.Categories.FirstOrDefault(c => c.Id == Id);
            //Product? delete3 = _db.Categories.Where(c => c.Id == Id).FirstOrDefault();
            _unitOfWork._productRepository.Remove(delete2);
            TempData["success"] = "Record deleted successfully";
            _unitOfWork.Save();
            return RedirectToAction("Index");
        }

    }
}
