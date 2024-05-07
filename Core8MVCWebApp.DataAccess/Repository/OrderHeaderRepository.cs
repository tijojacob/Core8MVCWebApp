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
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private readonly ApplicationDbContext _db;
        public OrderHeaderRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        //public void Save()
        //{
        //    _db.SaveChanges();
        //}

        public void Update(OrderHeader orderHeader)
        {
            _db.OrderHeaders.Update(orderHeader);
        }

        public void UpdateStatus(int id, string orderStatus, string? paymentStatus)
        {
            var orderFrmDb = _db.OrderHeaders.FirstOrDefault(u=>u.Id==id);
            if (orderFrmDb != null)
            {
                orderFrmDb.OrderStatus = orderStatus;
                if (!string.IsNullOrEmpty(paymentStatus))
                {
                    orderFrmDb.PaymentStatus = paymentStatus;
                }
            }
        }

        public void UpdateStipePaymentId(int id, string sessionId, string paymentIntentId)
        {
            var orderFrmDb = _db.OrderHeaders.FirstOrDefault(u => u.Id == id);
            if(!string.IsNullOrEmpty(sessionId))
            {
                orderFrmDb.SessionId = sessionId;
            }
            if(!string.IsNullOrEmpty (paymentIntentId))
            {
                orderFrmDb.PaymentIntentId = paymentIntentId;
                orderFrmDb.PaymentDate = DateTime.Now;
            }
        }
    }
}
