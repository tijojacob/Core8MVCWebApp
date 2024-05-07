﻿using Core8MVC.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core8MVC.DataAccess.Repository.IRepository
{
    public interface IOrderHeaderRepository : IRepository<OrderHeader>
    {
        void Update(OrderHeader orderHeader);
        //void Save();
        void UpdateStatus(int id, string orderStatus, string? paymentStatus = null);
        void UpdateStipePaymentId(int id, string sessionId, string paymentIntentId);

    }
}
