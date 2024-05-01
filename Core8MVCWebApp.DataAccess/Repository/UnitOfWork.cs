using Core8MVC.DataAccess.Repository.IRepository;
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

        private ApplicationDbContext _db;
        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            _categoryRepository = new CategoryRepository(_db);
            _productRepository = new ProductRepository(_db);
        }
        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
