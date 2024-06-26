﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core8MVC.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork
    {
        ICategoryRepository _categoryRepository { get; }
        IProductRepository _productRepository { get; }
        IProductImageRepository _productImageRepository { get; }
        ICompanyRepository _companyRepository { get; }
        IShoppingCartRepository _shoppingCartRepository { get; }
        IApplicationUserRepository _applicationUserRepository { get; }
        IOrderHeaderRepository _orderHeaderRepository { get; }
        IOrderDetailRepository _orderDetailRepository { get; }
        void Save();
    }
}
