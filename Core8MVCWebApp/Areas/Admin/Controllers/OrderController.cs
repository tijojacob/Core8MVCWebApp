using Core8MVC.DataAccess.Repository.IRepository;
using Core8MVC.Models.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace Core8MVCWebApp.Areas.Admin.Controllers
{
	[Area("Admin")]
	public class OrderController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
        public OrderController(IUnitOfWork unitOfWork)
        {
			_unitOfWork = unitOfWork;
        }
        public IActionResult Index()
		{
			return View();
		}

		#region API
		[HttpGet]
		public IActionResult GetAllOrders()
		{
			List<OrderHeader> orderHeaders = _unitOfWork._orderHeaderRepository.GetAll(includeProperties: "ApplicationUser").ToList();
			return Json(new { data = orderHeaders });
		}

		
		#endregion
	}
}
