using Core8MVC.DataAccess.Repository;
using Core8MVC.DataAccess.Repository.IRepository;
using Core8MVC.Models.Models;
using Core8MVC.Models.ViewModels;
using Core8MVC.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;

namespace Core8MVCWebApp.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    [BindProperties]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ShoppingCartVM ShoppingCartVM { get; set; }
        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new()
            {
                ShoppingCartList = _unitOfWork._shoppingCartRepository.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product"),
                OrderHeader = new()
            };
            foreach(var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.ItemPrice = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.ItemPrice * cart.Count);
            }
            return View(ShoppingCartVM);
        }

        public IActionResult PlusItem(int cartId)
        {
            ShoppingCart cartFrmDb = _unitOfWork._shoppingCartRepository.Get(u => u.Id == cartId);
            cartFrmDb.Count += 1;
            _unitOfWork._shoppingCartRepository.Update(cartFrmDb);
            _unitOfWork.Save();
            return RedirectToAction("Index");
        }

        public IActionResult MinusItem(int cartId)
        {
            ShoppingCart cartFrmDb = _unitOfWork._shoppingCartRepository.Get(u => u.Id == cartId);
            if (cartFrmDb.Count <= 1)
            {
                //remove Item
                _unitOfWork._shoppingCartRepository.Remove(cartFrmDb);
            }
            else
            {
                cartFrmDb.Count -= 1;
                _unitOfWork._shoppingCartRepository.Update(cartFrmDb);
            }
            _unitOfWork.Save();
            return RedirectToAction("Index");
        }


        public IActionResult RemoveItem(int cartId) 
        {
            ShoppingCart cartFrmDb = _unitOfWork._shoppingCartRepository.Get(u => u.Id == cartId);
            _unitOfWork._shoppingCartRepository.Remove(cartFrmDb);
            _unitOfWork.Save();
            return RedirectToAction("Index");
        }

        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new()
            {
                ShoppingCartList = _unitOfWork._shoppingCartRepository.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product"),
                OrderHeader = new()
            };

            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork._applicationUserRepository.Get(u=>u.Id==userId);
            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
            ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
            ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;


            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.ItemPrice = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.ItemPrice * cart.Count);
            }            

            return View(ShoppingCartVM);
        }
        [HttpPost]
        [ActionName("Summary")]
		public IActionResult SummaryPOST()
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM.ShoppingCartList = _unitOfWork._shoppingCartRepository.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product");

            ShoppingCartVM.OrderHeader.OrderDate = System.DateTime.Now;
			ShoppingCartVM.OrderHeader.ApplicationUserId = userId;
			ApplicationUser user= _unitOfWork._applicationUserRepository.Get(u => u.Id == userId);

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.ItemPrice = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.ItemPrice * cart.Count);
            }


            if (user.CompanyId.GetValueOrDefault(0)==0)
            {
                //its a regular cust account
                ShoppingCartVM.OrderHeader.PaymentStatus = StaticUtilities.PaymentStatusPending;
                ShoppingCartVM.OrderHeader.OrderStatus = StaticUtilities.StatusPending;
			}
            else
            {
				//a company user
				ShoppingCartVM.OrderHeader.PaymentStatus = StaticUtilities.PaymentStatusDelayedPayment;
				ShoppingCartVM.OrderHeader.OrderStatus = StaticUtilities.StatusApproved;
			}

            _unitOfWork._orderHeaderRepository.Add(ShoppingCartVM.OrderHeader);
            _unitOfWork.Save();			

			foreach (var cart in ShoppingCartVM.ShoppingCartList)
			{
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                    Price = cart.ItemPrice,
                    Count = cart.Count,

                };
                _unitOfWork._orderDetailRepository.Add(orderDetail);
				_unitOfWork.Save();				
			}

            if (user.CompanyId.GetValueOrDefault(0) == 0)
            {
                //its a regular cust account and Stripe Logic
                var domain = "https://localhost:7276/";//System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName;

                var options = new Stripe.Checkout.SessionCreateOptions
                {
                    SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                    CancelUrl = domain + "customer/cart/index",
                    LineItems = new List<Stripe.Checkout.SessionLineItemOptions>(),                    
                    Mode = "payment",
                };

                foreach(var item in ShoppingCartVM.ShoppingCartList)
                {
                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions()
                        {
                            UnitAmount = (long)(item.ItemPrice * 100),// $20.50=2050
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
                //SessionCustomerDetails CustomerDetails = new SessionCustomerDetails
                //{
                //    Name = user.Name,
                //    Email = user.Email,
                //    Phone = user.PhoneNumber,
                //    Address = new Stripe.Address
                //    {
                //        Line1 = user.StreetAddress,
                //        Line2 = user.StreetAddress,
                //        City = user.City,
                //        State = user.State,
                //        Country = user.City,
                //        PostalCode = user.PostalCode,
                //    }
                //};
                //session.CustomerDetails = CustomerDetails;
                _unitOfWork._orderHeaderRepository.UpdateStipePaymentId(ShoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
                _unitOfWork.Save();
                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);
            }

			return RedirectToAction(nameof(OrderConfirmation),new { id= ShoppingCartVM.OrderHeader.Id});
		}

        public IActionResult OrderConfirmation(int Id)
        {
            OrderHeader orderHeader = _unitOfWork._orderHeaderRepository.Get(u => u.Id == Id, includeProperties: "ApplicationUser");
            if(orderHeader.PaymentStatus!= StaticUtilities.PaymentStatusDelayedPayment)
            {   //this is a normal customer
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                if(session.PaymentStatus.ToLower()=="paid")
                {
                    _unitOfWork._orderHeaderRepository.UpdateStipePaymentId(Id, session.Id, session.PaymentIntentId);
                    _unitOfWork._orderHeaderRepository.UpdateStatus(Id, StaticUtilities.StatusApproved, StaticUtilities.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
            }
            List<ShoppingCart> shoppingCarts = _unitOfWork._shoppingCartRepository.GetAll(u =>u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();
            _unitOfWork._shoppingCartRepository.RemoveRange(shoppingCarts);
			_unitOfWork.Save();

			return View(Id);
        }

		private double GetPriceBasedOnQuantity(ShoppingCart shoppingCart)
        {
            if(shoppingCart.Count<=50)
            {
                return shoppingCart.Product.Price;
            }
            else if (shoppingCart.Count <= 100)
            {
                return shoppingCart.Product.Price50;
            }
            else //if (shoppingCart.Count > 100)
            {
                return shoppingCart.Product.Price100;
            }
        }

    }
}
