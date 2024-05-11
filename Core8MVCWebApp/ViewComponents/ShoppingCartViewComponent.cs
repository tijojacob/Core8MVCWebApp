using Core8MVC.DataAccess.Repository.IRepository;
using Core8MVC.Utility;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Core8MVCWebApp.ViewComponents
{
    public class ShoppingCartViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;
        public ShoppingCartViewComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                if (HttpContext.Session.GetInt32(StaticUtilities.SessionCart) == null)
                {
                    HttpContext.Session.SetInt32(StaticUtilities.SessionCart,
                    _unitOfWork._shoppingCartRepository.GetAll(u => u.ApplicationUserId == userId.Value).Count());
                }
                return View(HttpContext.Session.GetInt32(StaticUtilities.SessionCart));
            }
            else
            {
                HttpContext.Session.Clear();
                return View(0);
            }

        }
    }
}
