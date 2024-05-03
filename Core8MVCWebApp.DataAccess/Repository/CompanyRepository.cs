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
    public class CompanyRepository : Repository<Company>, ICompanyRepository
    {
        private readonly ApplicationDbContext _db;
        public CompanyRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(Company company)
        {
            _db.Companies.Update(company);
            var dbObject = _db.Companies.FirstOrDefault(u=>u.Id == company.Id);
            if(dbObject != null)
            {
                dbObject.Id= company.Id;
                dbObject.Name = company.Name;
                dbObject.PhoneNumber = company.PhoneNumber;
                dbObject.PostalCode = company.PostalCode;
                dbObject.State = company.State;
                dbObject.StreetAddress = company.StreetAddress;                
            }
        }
    }
}
