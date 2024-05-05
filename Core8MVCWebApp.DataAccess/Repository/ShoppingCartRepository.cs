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
    public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
    {
        private readonly ApplicationDbContext _db;
        public ShoppingCartRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(ShoppingCart shoppingCart)
        {
            _db.ShoppingCarts.Update(shoppingCart);
            var dbObject = _db.ShoppingCarts.FirstOrDefault(u=>u.Id == shoppingCart.Id);
            if(dbObject != null)
            {
                dbObject.ProductId=shoppingCart.ProductId;
                dbObject.ApplicationUserId = shoppingCart.ApplicationUserId;
                dbObject.Count = shoppingCart.Count;                
            }
        }
    }
}
