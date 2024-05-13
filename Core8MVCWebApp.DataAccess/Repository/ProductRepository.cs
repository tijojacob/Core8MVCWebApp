using Core8MVC.DataAccess.Repository.IRepository;
using Core8MVC.Models.Models;
using Core8MVCWebApp.Controllers.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core8MVC.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _db;
        public ProductRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(Product product)
        {
            _db.Products.Update(product);
            var dbObject = _db.Products.FirstOrDefault(u=>u.Id == product.Id);
            if(dbObject != null)
            {
                dbObject.Title=product.Title;
                dbObject.Description = product.Description;
                dbObject.ISBN = product.ISBN;
                dbObject.Author = product.Author;
                dbObject.ListPrice = product.ListPrice;
                dbObject.Price = product.Price;
                dbObject.Price50 = product.Price50;
                dbObject.Price100 = product.Price100;
                dbObject.CategoryId = product.CategoryId;
                //if (product.ImageURL != null)
                //{
                //    dbObject.ImageURL = product.ImageURL;
                //}
            }
        }
    }
}
