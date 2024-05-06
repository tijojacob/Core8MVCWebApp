using Microsoft.AspNetCore.Mvc;
using Core8MVC.Models.Models;
using Core8MVC.Models.ViewModels;
using Core8MVCWebApp.Controllers.Data;
using Core8MVC.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using Core8MVC.Utility;
using Microsoft.AspNetCore.Authorization;

namespace Core8MVCWebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = StaticUtilities.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            List<Product> objProductList = _unitOfWork._productRepository.GetAll(includeProperties : "Category").ToList();
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
                product.Product = _unitOfWork._productRepository.Get(c => c.Id == Id, includeProperties: "Category");
            }
            else
            {
                product.Product = new Product();
            }            
            return View(product);
        }

        [HttpPost]
        public IActionResult UpsertProduct(ProductVM obj, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRoot = _webHostEnvironment.WebRootPath;

                if (file!=null)
                {
                    if(!string.IsNullOrEmpty(obj.Product.ImageURL))
                    {
                        var oldImagePath = Path.Combine(wwwRoot, obj.Product.ImageURL.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);                            
                        }
                        obj.Product.ImageURL = string.Empty;
                    }

                    if(string.IsNullOrEmpty(obj.Product.ImageURL))
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetFileNameWithoutExtension(file.FileName) + Path.GetExtension(file.FileName);
                        string filePath = Path.Combine(wwwRoot, @"images\product");

                        using (var fileStream = new FileStream(Path.Combine(filePath, fileName), FileMode.Create))
                        {
                            file.CopyTo(fileStream);
                        }
                        obj.Product.ImageURL = @"images\product\" + fileName;

                    }                                        
                }

                if(obj.Product.Id==0)
                {
                    _unitOfWork._productRepository.Add(obj.Product);
                    TempData["success"] = "Record created successfully";
                }
                else
                {
                    _unitOfWork._productRepository.Update(obj.Product);
                    TempData["success"] = "Record updated successfully";
                }
                
                
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

        #region API
        [HttpGet]
        public IActionResult GetAllProducts()
        {
            List<Product> objProductList = _unitOfWork._productRepository.GetAll(includeProperties: "Category").ToList();
            return Json(new { data = objProductList });
        }

        [HttpDelete]
        public IActionResult Delete(int? Id)
        {
            Product productDelete = _unitOfWork._productRepository.Get(c => c.Id == Id);
            if (productDelete == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            string wwwRoot = _webHostEnvironment.WebRootPath;
            var oldImagePath = Path.Combine(wwwRoot, productDelete.ImageURL.TrimStart('\\'));
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }

            _unitOfWork._productRepository.Remove(productDelete);
            TempData["success"] = "Record deleted successfully";
            _unitOfWork.Save();
            
            return Json(new { success = true, message = "Record deleted successfully" });            
        }
        #endregion

    }
}
