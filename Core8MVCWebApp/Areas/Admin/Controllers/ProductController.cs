using Microsoft.AspNetCore.Mvc;
using Core8MVC.Models.Models;
using Core8MVC.Models.ViewModels;
using Core8MVCWebApp.Controllers.Data;
using Core8MVC.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc.Rendering;

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
        public IActionResult UpsertProduct(int? Id)
        {
            ProductVM product = new ProductVM();
            IEnumerable<SelectListItem> CategoryList = _unitOfWork._categoryRepository.GetAll()
                .Select(u => new SelectListItem { Text = u.Name, Value = u.Id.ToString() });
            //ViewBag.CategoryList = CategoryList;
            //ViewData["CategoryList"]=CategoryList;
            product.Categories = CategoryList;

            if ( Id != null && Id>0)
            {
                product.Product = _unitOfWork._productRepository.Get(c => c.Id == Id);
            }
            else
            {
                product.Product = new Product();
            }            
            return View(product);
        }

        [HttpPost]
        public IActionResult CreateProduct(ProductVM obj, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork._productRepository.Add(obj.Product);
                TempData["success"] = "Record created successfully";
                _unitOfWork.Save();
                return RedirectToAction("Index");
            }
            else
            {
                IEnumerable<SelectListItem> CategoryList = _unitOfWork._categoryRepository.GetAll()
                .Select(u => new SelectListItem { Text = u.Name, Value = u.Id.ToString() });
                //ViewBag.CategoryList = CategoryList;
                //ViewData["CategoryList"]=CategoryList;

                obj.Categories = CategoryList;
                
            }
            return View(obj);

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
