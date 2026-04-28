using APM.DbEntities;
using APM.DbEntities.Base;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace APM.ConTaxi.Taxi
{
    public interface IConTaxiService
    {
        public object? Get(string entityName, Guid id);
        public T? Get<T>(Guid id) where T : APMBaseEntity;
        public T? FirstOrDefault<T>(Func<T, bool>? selector = null) where T : APMBaseEntity;
        public int Total<T>() where T : APMBaseEntity;
        public IEnumerable<T> GetDataSetQuery<T>(
            Func<T, bool>? where = null,
            int pageIndex = 1,
            int pageSize = 10,
            bool paging = true,
            Func<T, object>? orderBy = null,
            bool descending = false) where T : APMBaseEntity;
        public Dictionary<User, List<UserRole>> UserLogin(string username);
        public int Transaction<T>(T entity, EntityState entityState) where T : BaseEntity;
        public int Transaction<T>(IEnumerable<T> entities, EntityState entityState) where T : BaseEntity;
        public int Transaction<T>(IEnumerable<T> entities, IDictionary<Guid, EntityState> entitiesState) where T : BaseEntity;
        public void Migrate();
        public T Create<T>(T entity) where T : BaseEntity;
        public int Create<T>(IEnumerable<T> entities) where T : BaseEntity;
        public T Update<T>(T entity) where T : BaseEntity;
        public int Delete<T>(Guid id) where T : BaseEntity;
        public int Delete<T>(IEnumerable<Guid> ids) where T : BaseEntity;
        public int Delete<T>(Func<T, bool> where) where T : BaseEntity;
    }
}
