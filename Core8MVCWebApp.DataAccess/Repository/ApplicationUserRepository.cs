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
    public class ApplicationUserRepository : Repository<ApplicationUser>, IApplicationUserRepository
    {
        private readonly ApplicationDbContext _db;
        public ApplicationUserRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        //public void Save()
        //{
        //    _db.SaveChanges();
        //}

        //public void Update(ApplicationUser applicationUser)
        //{
        //    _db.Categories.Update(ApplicationUser);
        //}
    }
}
