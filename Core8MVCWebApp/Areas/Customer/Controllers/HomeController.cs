using Core8MVC.DataAccess.Repository.IRepository;
using Core8MVC.Models.Models;
using Core8MVC.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace Core8MVCWebApp.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            List<Product> objProductList = _unitOfWork._productRepository.GetAll(includeProperties: "Category").ToList();
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if(userId!=null)
            {
                HttpContext.Session.SetInt32(StaticUtilities.SessionCart,
                    _unitOfWork._shoppingCartRepository.GetAll(u => u.ApplicationUserId == userId.Value).Count());
            }
            else
            {
                HttpContext.Session.SetInt32(StaticUtilities.SessionCart, 0);
            }
            

            return View(objProductList);
        }

        public IActionResult Details(int productId)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            ShoppingCart cartFrmDb = _unitOfWork._shoppingCartRepository.Get(u => u.ProductId == productId && u.ApplicationUserId == userId);
            ShoppingCart objCart = new()
            {
                Id = cartFrmDb != null ? cartFrmDb.Id : 0,
                Product = _unitOfWork._productRepository.Get(u => u.Id == productId, includeProperties: "Category"),
                Count = 1,
                ProductId = productId,
                ApplicationUserId = userId
            };
            return View(objCart);
        }
        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart cart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCart cartFrmDb = _unitOfWork._shoppingCartRepository.Get(u=>u.ProductId==cart.ProductId && u.ApplicationUserId== userId);

            if(cartFrmDb!=null)
            {
                cartFrmDb.Count += cart.Count;
                _unitOfWork._shoppingCartRepository.Update(cartFrmDb);
            }
            else
            {
                cart.ApplicationUserId = userId;
                _unitOfWork._shoppingCartRepository.Add(cart);
                
            }        
            _unitOfWork.Save();
            HttpContext.Session.SetInt32(StaticUtilities.SessionCart,
                    _unitOfWork._shoppingCartRepository.GetAll(u => u.ApplicationUserId == userId).Count());

            cart.Product = _unitOfWork._productRepository.Get(u => u.Id == cart.ProductId, includeProperties: "Category");

            TempData["success"] = "Cart updated successfully";
            return View(cart);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
