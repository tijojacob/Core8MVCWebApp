﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Core8MVC.DataAccess.Repository.IRepository
{
    public interface IRepository<T> where T:class
    {
        IEnumerable<T> GetAll(string? includeProperties = null);
        T Get(Expression<Func<T,bool>>filter, string? includeProperties = null);
        void Add(T item);
        //void Update(T item);
        void Remove(T item);
        void RemoveRange(IEnumerable<T> items);

    }
}
