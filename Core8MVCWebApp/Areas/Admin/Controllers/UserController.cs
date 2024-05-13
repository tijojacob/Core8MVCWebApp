using Microsoft.AspNetCore.Mvc;
using Core8MVC.Models.Models;
using Core8MVC.DataAccess.Repository.IRepository;
using Core8MVC.Utility;
using Microsoft.AspNetCore.Authorization;
using Core8MVCWebApp.Controllers.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Core8MVC.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;

namespace Core8MVCWebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = StaticUtilities.Role_Admin)]
    public class UserController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public UserController(IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _unitOfWork = unitOfWork;
            _userManager= userManager;
            _roleManager= roleManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult RoleManagement(string userId)
        {
            //var roleId = _unitOfWork..UserRoles.FirstOrDefault(u => u.UserId == userId).RoleId;

            RoleManagementVM roleManagementVM = new RoleManagementVM()
            {
                ApplicationUser = _unitOfWork._applicationUserRepository.Get(u => u.Id == userId, includeProperties:"Company"),
                RoleList = _roleManager.Roles.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Name
                }),
                CompanyList = _unitOfWork._companyRepository.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }),
            };

            roleManagementVM.ApplicationUser.Role = _userManager.GetRolesAsync(_unitOfWork._applicationUserRepository.Get(u => u.Id == userId)).GetAwaiter().GetResult().FirstOrDefault();// _db.Roles.FirstOrDefault(u => u.Id == roleId).Name;

            return View(roleManagementVM);
        }
        [HttpPost]
        public IActionResult RoleManagement(RoleManagementVM roleManagementVM)
        {
            //var roleId = _db.UserRoles.FirstOrDefault(u => u.UserId == roleManagementVM.ApplicationUser.Id).RoleId;
            string oldRole = _userManager.GetRolesAsync(_unitOfWork._applicationUserRepository.Get(u => u.Id == roleManagementVM.ApplicationUser.Id)).GetAwaiter().GetResult().FirstOrDefault();
            //_db.Roles.FirstOrDefault(u => u.Id == roleId).Name;

            ApplicationUser applicationUser = _unitOfWork._applicationUserRepository.Get(u => u.Id == roleManagementVM.ApplicationUser.Id);

            if (roleManagementVM.ApplicationUser.Role != oldRole)
            {
                // role is updated
                if(roleManagementVM.ApplicationUser.Role== StaticUtilities.Role_Company)
                {
                    applicationUser.CompanyId = roleManagementVM.ApplicationUser.CompanyId;
                }
                if(oldRole == StaticUtilities.Role_Company)
                {
                    applicationUser.CompanyId = null;
                }
                _unitOfWork._applicationUserRepository.Update(applicationUser);
                _unitOfWork.Save();

                _userManager.RemoveFromRoleAsync(applicationUser,oldRole).GetAwaiter().GetResult();
                _userManager.AddToRoleAsync(applicationUser, roleManagementVM.ApplicationUser.Role).GetAwaiter().GetResult();

            }
            else
            {
                if(oldRole == StaticUtilities.Role_Company && applicationUser.CompanyId != roleManagementVM.ApplicationUser.CompanyId)
                {
                    applicationUser.CompanyId = roleManagementVM.ApplicationUser.CompanyId;
                    _unitOfWork._applicationUserRepository.Update(applicationUser);
                    _unitOfWork.Save();
                }
            }

            return RedirectToAction("Index");
        }

        #region API
        [HttpGet]
        public IActionResult GetAllUsers()
        {
            List<ApplicationUser> objUserList = _unitOfWork._applicationUserRepository.GetAll(includeProperties:"Company").ToList();

            //var roles = _db.Roles.ToList();

            //var userRoles = _db.UserRoles.ToList();

            foreach(var user in objUserList)
            {
                //var roleId = userRoles.FirstOrDefault(u => u.UserId == user.Id).RoleId;
                user.Role = _userManager.GetRolesAsync(user).GetAwaiter().GetResult().FirstOrDefault();//_db.Roles.FirstOrDefault(u=>u.Id==roleId).Name;
                if(user.Company == null)
                {
                    user.Company = new Company()
                    {
                        Name = ""
                    };
                }
                if(user.LockoutEnd == null)
                {
                    user.LockoutEnd = (DateTime?)null;
                }
            }
            return Json(new { data = objUserList });
        }

        [HttpPost]
        public IActionResult LockUnlock([FromBody]string Id)
        {
            var status = "";
            var objUser = _unitOfWork._applicationUserRepository.Get(u => u.Id==Id);
            if(objUser == null)
            {
                return Json(new { success = false, message = "Error in lock/unlock user" });
            }
            if(objUser.LockoutEnd!=null && objUser.LockoutEnd>DateTime.Now)
            {
                //usr is locked and need to unlock
                objUser.LockoutEnd=DateTime.Now;
                status = "unlocked";
            }
            else
            {
                //usr needs to be locked
                objUser.LockoutEnd = DateTime.Now.AddYears(1);
                status = "locked";
            }
            _unitOfWork._applicationUserRepository.Update(objUser);
            _unitOfWork.Save();
            return Json(new { success = true, message = "User "+ status  + " successfully" });            
        }
        #endregion

    }
}
