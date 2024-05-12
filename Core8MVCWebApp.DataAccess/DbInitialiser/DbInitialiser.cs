using Core8MVC.Models.Models;
using Core8MVC.Utility;
using Core8MVCWebApp.Controllers.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core8MVC.DataAccess.DbInitialiser
{
    public class DbInitialiser : IDBInitialiser
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;
        public DbInitialiser(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext db)
        {
            _userManager=userManager;
            _roleManager=roleManager;
            _db=db;
        }
        public void Initialise()
        {
            //migration if they are not applied
            try
            {
                if(_db.Database.GetPendingMigrations().Count()>0)
                {
                    _db.Database.Migrate();
                }
            }
            catch(Exception ex){ }

            //create role if not created
            if (!_roleManager.RoleExistsAsync(StaticUtilities.Role_Customer).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(StaticUtilities.Role_Customer)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(StaticUtilities.Role_Admin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(StaticUtilities.Role_Company)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(StaticUtilities.Role_Employee)).GetAwaiter().GetResult();

                //create admin user, if roles are not available
                _userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "admin@app.com",
                    Email = "admin@app.com",
                    Name = "Admin",
                    PhoneNumber = "1234567890",
                    StreetAddress = "Test Street",
                    State = "IL",
                    PostalCode = "1234567890",
                    City = "Chicago"
                }, "Admin@123").GetAwaiter().GetResult();

                ApplicationUser user = _db.ApplicationUsers.FirstOrDefault(u => u.Email == "admin@app.com");
                _userManager.AddToRoleAsync(user, StaticUtilities.Role_Admin).GetAwaiter().GetResult();

            }


        }
    }
}
