using Core8MVC.DataAccess.Repository.IRepository;
using Core8MVC.Models.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

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
            return View(objProductList);
        }

        public IActionResult Details(int productId)
        {
            Product objProduct = _unitOfWork._productRepository.Get(u=>u.Id== productId, includeProperties: "Category");
            return View(objProduct);
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
