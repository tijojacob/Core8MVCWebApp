using Core8MVC.DataAccess.Repository.IRepository;
using Core8MVC.Models.Models;
using Core8MVC.Models.ViewModels;
using Core8MVC.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
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
        [Authorize]
        [ActionName("Details")]
        public IActionResult Details_Pay_Now()
        {
            orderVM.orderHeader = _unitOfWork._orderHeaderRepository.Get(u => u.Id == orderVM.orderHeader.Id, includeProperties: "ApplicationUser");
            orderVM.orderDetails = _unitOfWork._orderDetailRepository.GetAll(u => u.OrderHeaderId == orderVM.orderHeader.Id, includeProperties: "Product");

            var domain = Request.Scheme + "://" + Request.Host.Value + "/";//System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName;

            var options = new Stripe.Checkout.SessionCreateOptions
            {
                SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderId={orderVM.orderHeader.Id}",
                CancelUrl = domain + $"admin/order/details?orderId={orderVM.orderHeader.Id}",
                LineItems = new List<Stripe.Checkout.SessionLineItemOptions>(),
                Mode = "payment",
            };

            foreach (var item in orderVM.orderDetails)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions()
                    {
                        UnitAmount = (long)(item.Price * 100),// $20.50=2050
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title
                        }
                    },
                    Quantity = item.Count,
                };
                options.LineItems.Add(sessionLineItem);
            }
            var service = new Stripe.Checkout.SessionService();
            Session session = service.Create(options);
            _unitOfWork._orderHeaderRepository.UpdateStipePaymentId(orderVM.orderHeader.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        public IActionResult PaymentConfirmation(int orderHeaderId)
        {
            OrderHeader orderHeader = _unitOfWork._orderHeaderRepository.Get(u => u.Id == orderHeaderId, includeProperties: "ApplicationUser");
            if (orderHeader.PaymentStatus == StaticUtilities.PaymentStatusDelayedPayment)
            {   //this is a normal customer
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork._orderHeaderRepository.UpdateStipePaymentId(orderHeaderId, session.Id, session.PaymentIntentId);
                    _unitOfWork._orderHeaderRepository.UpdateStatus(orderHeaderId, orderHeader.OrderStatus, StaticUtilities.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
            }
            
            return View(orderHeaderId);
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

            TempData["Success"] = "Order Status updated successfully";

            return RedirectToAction(nameof(Details), new { orderId = orderVM.orderHeader.Id });
        }
        
        
        [HttpPost]
        [Authorize(Roles = StaticUtilities.Role_Admin + "," + StaticUtilities.Role_Employee)]
        public IActionResult ShipOrder()
        {
			var orderFrmDb = _unitOfWork._orderHeaderRepository.Get(u => u.Id == orderVM.orderHeader.Id);
            orderFrmDb.TrackingNumber = orderVM.orderHeader.TrackingNumber;
            orderFrmDb.Carrier = orderVM.orderHeader.Carrier;
            _unitOfWork._orderHeaderRepository.Update(orderFrmDb);
			_unitOfWork._orderHeaderRepository.UpdateStatus(orderVM.orderHeader.Id, StaticUtilities.StatusShipped);
            _unitOfWork.Save();

            TempData["Success"] = "Order Shipped successfully";

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
