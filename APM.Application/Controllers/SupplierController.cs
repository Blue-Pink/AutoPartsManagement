using APM.DbEntities;
using APM.DbEntities.DTOs;
using APM.IBusiness;
using APM.UtilEntities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace APM.Application.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class SupplierController(ISupplierService supplierService) : APMController
    {
        [HttpGet, Route("[action]")]
        public UsualApiData<Supplier> GetSuppliers(int pageIndex = 1, int pageSize = 10)
        {
            return UsualResult(supplierService.GetSuppliers(pageIndex, pageSize));
        }

        /// <summary>
        /// 新增或更新供应商，接收 SupplierDTO
        /// </summary>
        [HttpPost, Route("[action]")]
        public UsualApiData<Supplier?> EditSupplier([FromBody] SupplierDTO supplier)
        {
            if (supplier is null)
                throw new APMException("参数 supplier 不可为空");

            var result = supplierService.EditSupplier(supplier);
            return UsualResult(result);
        }
    }
}