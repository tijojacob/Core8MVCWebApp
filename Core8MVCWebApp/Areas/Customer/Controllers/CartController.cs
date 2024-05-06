﻿using Core8MVC.DataAccess.Repository;
using Core8MVC.DataAccess.Repository.IRepository;
using Core8MVC.Models.Models;
using Core8MVC.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Core8MVCWebApp.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
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
                OrderTotal = 0
            };
            foreach(var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.ItemPrice = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderTotal += (cart.ItemPrice * cart.Count);
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
            return View();
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
