using APM.DbEntities;
using APM.DbEntities.DTOs;
using APM.UtilEntities;
using System;
using System.Collections.Generic;

namespace APM.IBusiness
{
    public interface IInboundOrderService
    {
        public PagingData<InboundOrder> GetInboundOrders(int pageIndex = 1, int pageSize = 10);
        public InboundOrder? GetInboundOrder(Guid id);

        /// <summary>
        /// 使用 DTO 新增或更新入库单（包含明细）
        /// </summary>
        public InboundOrder EditInboundOrder(InboundOrderDTO dto);
    }
}