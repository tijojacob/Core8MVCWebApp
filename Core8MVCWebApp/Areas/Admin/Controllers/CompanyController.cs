using Microsoft.AspNetCore.Mvc;
using Core8MVC.Models.Models;
using Core8MVC.DataAccess.Repository.IRepository;
using Core8MVC.Utility;
using Microsoft.AspNetCore.Authorization;

namespace Core8MVCWebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = StaticUtilities.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public CompanyController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            List<Company> objCompanyList = _unitOfWork._companyRepository.GetAll().ToList();
            return View(objCompanyList);
        }
        public IActionResult UpsertCompany(int? Id)
        {
            Company company = new Company();
            if ( Id != null && Id>0)
            {
                company = _unitOfWork._companyRepository.Get(c => c.Id == Id);
            }
            else
            {
                company = new Company();
            }            
            return View(company);
        }

        [HttpPost]
        public IActionResult UpsertCompany(Company obj)
        {
            if (ModelState.IsValid)
            {
                if(obj.Id==0)
                {
                    _unitOfWork._companyRepository.Add(obj);
                    TempData["success"] = "Record created successfully";
                }
                else
                {
                    _unitOfWork._companyRepository.Update(obj);
                    TempData["success"] = "Record updated successfully";
                }
                
                
                _unitOfWork.Save();
                return RedirectToAction("Index");
            }
            
            return View(obj);

        }

        public IActionResult DeleteCompany(int? Id)
        {
            if (Id == null || Id == 0)
            {
                ModelState.AddModelError("", "Select a valid Record");
                //return RedirectToAction("CreateCompany");
            }
            else if (Id > 0)
            {
                Company? delete2 = _unitOfWork._companyRepository.Get(c => c.Id == Id);


                //Company? delete1 = _db.Categories.Find(Id);
                //Company? delete2 = _db.Categories.FirstOrDefault(c => c.Id == Id);
                //Company? delete3 = _db.Categories.Where(c => c.Id == Id).FirstOrDefault();

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
        [HttpPost, ActionName("DeleteCompany")]
        public IActionResult DeleteCompanyPOST(int? Id)
        {
            Company? delete2 = _unitOfWork._companyRepository.Get(c => c.Id == Id);


            //Company? delete1 = _db.Categories.Find(Id);
            //Company? delete2 = _db.Categories.FirstOrDefault(c => c.Id == Id);
            //Company? delete3 = _db.Categories.Where(c => c.Id == Id).FirstOrDefault();
            _unitOfWork._companyRepository.Remove(delete2);
            TempData["success"] = "Record deleted successfully";
            _unitOfWork.Save();
            return RedirectToAction("Index");
        }

        #region API
        [HttpGet]
        public IActionResult GetAllCompany()
        {
            List<Company> objCompanyList = _unitOfWork._companyRepository.GetAll().ToList();
            return Json(new { data = objCompanyList });
        }

        [HttpDelete]
        public IActionResult Delete(int? Id)
        {
            Company CompanyDelete = _unitOfWork._companyRepository.Get(c => c.Id == Id);
            if (CompanyDelete == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            
            _unitOfWork._companyRepository.Remove(CompanyDelete);
            TempData["success"] = "Record deleted successfully";
            _unitOfWork.Save();
            
            return Json(new { success = true, message = "Record deleted successfully" });            
        }
        #endregion

    }
}
