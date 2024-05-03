using System;
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
        ICompanyRepository _companyRepository { get; }
        void Save();
    }
}
