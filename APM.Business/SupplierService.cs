using System;
using System.Collections.Generic;
using System.Linq;
using APM.ConTaxi.Taxi;
using APM.DbEntities;
using APM.DbEntities.DTOs;
using APM.IBusiness;
using APM.UtilEntities;

namespace APM.Business
{
    public class SupplierService(IConTaxiService taxi) : ISupplierService
    {
        public PagingData<Supplier> GetSuppliers(int pageIndex = 1, int pageSize = 10)
        {
            var list = taxi.GetDataSetQuery<Supplier>(pageIndex: pageIndex, pageSize: pageSize).ToList();
            var total = taxi.Total<Supplier>();
            return new PagingData<Supplier>(list, total, pageIndex, pageSize);
        }

        public Supplier? GetSupplier(Guid id)
        {
            if (id == Guid.Empty) return null;
            return taxi.Get<Supplier>(id);
        }

        public Supplier EditSupplier(SupplierDTO supplierDto)
        {
            if (supplierDto is null)
                throw new APMException("参数 supplier 不可为空");

            // 新增
            if (supplierDto.Id == Guid.Empty || supplierDto.Id == null || taxi.Get<Supplier>(supplierDto.Id.Value) == null)
            {
                var toCreate = new Supplier
                {
                    Name = supplierDto.Name ?? throw new APMException("供应商名称不可为空"),
                    Contact = supplierDto.Contact,
                    Phone = supplierDto.Phone,
                    Address = supplierDto.Address
                };
                var created = taxi.Create(toCreate);
                return created;
            }

            // 更新
            var existing = taxi.Get<Supplier>(supplierDto.Id.Value) ?? throw new APMException("要更新的供应商不存在");
            existing.Name = supplierDto.Name ?? existing.Name;
            existing.Contact = supplierDto.Contact ?? existing.Contact;
            existing.Phone = supplierDto.Phone ?? existing.Phone;
            existing.Address = supplierDto.Address ?? existing.Address;

            var updated = taxi.Update(existing);
            return updated;
        }

    }
}