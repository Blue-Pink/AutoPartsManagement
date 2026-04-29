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
    public class InboundOrderController(IInboundOrderService inboundOrderService) : APMController
    {
        [HttpGet, Route("[action]")]
        public UsualApiData<InboundOrder> GetInboundOrders(int pageIndex = 1, int pageSize = 10)
        {
            return UsualResult(inboundOrderService.GetInboundOrders(pageIndex, pageSize));
        }

        [HttpPost, Route("[action]")]
        public UsualApiData<InboundOrder?> EditInboundOrder([FromBody] InboundOrderDTO dto)
        {
            if (dto is null)
                throw new APMException("参数 dto 不可为空");

            var result = inboundOrderService.EditInboundOrder(dto);
            return UsualResult(result);
        }
    }
}