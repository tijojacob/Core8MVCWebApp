﻿using Core8MVC.DataAccess.Repository.IRepository;
using Core8MVC.Models.Models;
using Core8MVCWebApp.Controllers.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core8MVC.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        public ICategoryRepository _categoryRepository { get; private set; }
        public IProductRepository _productRepository { get; private set; }
        public ICompanyRepository _companyRepository { get; private set; }
        public IShoppingCartRepository _shoppingCartRepository { get; private set; }
        public IApplicationUserRepository _applicationUserRepository { get; private set; }
        public IOrderHeaderRepository _orderHeaderRepository { get; private set; }
        public IOrderDetailRepository _orderDetailRepository { get; private set; }

        private ApplicationDbContext _db;
        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            _categoryRepository = new CategoryRepository(_db);
            _productRepository = new ProductRepository(_db);
            _companyRepository = new CompanyRepository(_db);
            _shoppingCartRepository = new ShoppingCartRepository(_db);
            _applicationUserRepository = new ApplicationUserRepository(_db);
            _orderHeaderRepository = new OrderHeaderRepository(_db);
            _orderDetailRepository = new OrderDetailRepository(_db);
        }
        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
