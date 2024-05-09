using Core8MVC.DataAccess.Repository.IRepository;
using Core8MVC.Models.Models;
using Core8MVC.Models.ViewModels;
using Core8MVC.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using System.Diagnostics;
using System.Security.Claims;

namespace Core8MVCWebApp.Areas.Admin.Controllers
{
	[Area("Admin")]
    [Authorize]
    public class OrderController : Controller
	{
        [BindProperty]
        public OrderVM orderVM { get; set; }

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
		public IActionResult GetAllOrders(string? status=null)
		{
			IEnumerable<OrderHeader> orderHeaders;

            if(User.IsInRole(StaticUtilities.Role_Admin)||User.IsInRole(StaticUtilities.Role_Employee))
            {
                orderHeaders = _unitOfWork._orderHeaderRepository.GetAll(includeProperties: "ApplicationUser").ToList();
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                orderHeaders = _unitOfWork._orderHeaderRepository.GetAll(u => u.ApplicationUserId==userId ,includeProperties: "ApplicationUser").ToList();
            }

            switch (status)
            {
                case "pending":
                    orderHeaders = orderHeaders.Where(u=>u.PaymentStatus==StaticUtilities.PaymentStatusDelayedPayment);
                    break;
                case "inprocess":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == StaticUtilities.StatusInProcess);
                    break;
                case "completed":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == StaticUtilities.StatusShipped);
                    break;
                case "approved":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == StaticUtilities.StatusApproved);
                    break;
                default:
                    break;
            }

            return Json(new { data = orderHeaders });
		}

        public IActionResult Details(int orderId)
        {
            orderVM = new()
            {
                orderHeader = _unitOfWork._orderHeaderRepository.Get(u => u.Id == orderId, includeProperties: "ApplicationUser"),
                orderDetails = _unitOfWork._orderDetailRepository.GetAll(u => u.OrderHeaderId == orderId, includeProperties: "Product")
            };
            return View(orderVM);
        }
        [HttpPost]
        [Authorize(Roles =StaticUtilities.Role_Admin+","+StaticUtilities.Role_Employee)]
        public IActionResult UpdateOrderDetails()
        {
            var orderFrmDb = _unitOfWork._orderHeaderRepository.Get(u => u.Id == orderVM.orderHeader.Id);
            orderFrmDb.Name = orderVM.orderHeader.Name;
            orderFrmDb.PhoneNumber = orderVM.orderHeader.PhoneNumber;
            orderFrmDb.StreetAddress = orderVM.orderHeader.StreetAddress;
            orderFrmDb.City = orderVM.orderHeader.City;
            orderFrmDb.State = orderVM.orderHeader.State;
            orderFrmDb.PostalCode = orderVM.orderHeader.PostalCode;
            if(!string.IsNullOrEmpty(orderVM.orderHeader.Carrier))
            {
                orderFrmDb.Carrier = orderVM.orderHeader.Carrier;
            }
            if (!string.IsNullOrEmpty(orderVM.orderHeader.TrackingNumber))
            {
                orderFrmDb.TrackingNumber = orderVM.orderHeader.TrackingNumber;
            }
            _unitOfWork._orderHeaderRepository.Update(orderFrmDb);
            _unitOfWork.Save();

            TempData["Success"] = "Order updated successfully";

            return RedirectToAction(nameof(Details),new {orderId=orderFrmDb.Id});
        }

        
        [HttpPost]
        [Authorize(Roles = StaticUtilities.Role_Admin + "," + StaticUtilities.Role_Employee)]
        public IActionResult StartProcessing()
        {
            var orderFrmDb = _unitOfWork._orderHeaderRepository.Get(u => u.Id == orderVM.orderHeader.Id);
            orderFrmDb.TrackingNumber = orderVM.orderHeader.TrackingNumber;
            orderFrmDb.Carrier = orderVM.orderHeader.Carrier;
            orderFrmDb.ShippingDate = DateTime.Now;
            if(orderFrmDb.PaymentStatus == StaticUtilities.PaymentStatusDelayedPayment)
            {
                orderFrmDb.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            }
            _unitOfWork._orderHeaderRepository.Update(orderFrmDb);
            _unitOfWork._orderHeaderRepository.UpdateStatus(orderVM.orderHeader.Id, StaticUtilities.StatusInProcess);
            _unitOfWork.Save();

            TempData["Success"] = "Order Shipped successfully";

            return RedirectToAction(nameof(Details), new { orderId = orderVM.orderHeader.Id });
        }
        
        
        [HttpPost]
        [Authorize(Roles = StaticUtilities.Role_Admin + "," + StaticUtilities.Role_Employee)]
        public IActionResult ShipOrder()
        {
            _unitOfWork._orderHeaderRepository.UpdateStatus(orderVM.orderHeader.Id, StaticUtilities.StatusShipped);
            _unitOfWork.Save();

            TempData["Success"] = "Records updated successfully";

            return RedirectToAction(nameof(Details), new { orderId = orderVM.orderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = StaticUtilities.Role_Admin + "," + StaticUtilities.Role_Employee)]
        public IActionResult CancelOrder()
        {
            var orderFrmDb = _unitOfWork._orderHeaderRepository.Get(u => u.Id == orderVM.orderHeader.Id);
            if (orderFrmDb.OrderStatus == StaticUtilities.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderFrmDb.PaymentIntentId
                };
                var service = new RefundService();
                Refund refund = service.Create(options);
                _unitOfWork._orderHeaderRepository.UpdateStatus(orderVM.orderHeader.Id, StaticUtilities.StatusCancelled, StaticUtilities.StatusRefunded);
            }            
            else
            {
                _unitOfWork._orderHeaderRepository.UpdateStatus(orderVM.orderHeader.Id, StaticUtilities.StatusCancelled, StaticUtilities.StatusCancelled);
            }
            _unitOfWork.Save();

            TempData["Success"] = "Order cancelled successfully";

            return RedirectToAction(nameof(Details), new { orderId = orderVM.orderHeader.Id });
        }
        #endregion
    }
}
