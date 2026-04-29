using APM.DbEntities;
using APM.DbEntities.DTOs;
using APM.UtilEntities;
using System;
using System.Collections.Generic;

namespace APM.IBusiness
{
    public interface ISupplierService
    {
        public PagingData<Supplier> GetSuppliers(int pageIndex = 1, int pageSize = 10);
        public Supplier? GetSupplier(Guid id);

        /// <summary>
        /// 使用 SupplierDTO 进行新增或更新，若 Id 为空或数据库中不存在则新增，否则更新并返回实体
        /// </summary>
        public Supplier EditSupplier(SupplierDTO supplier);

    }
}