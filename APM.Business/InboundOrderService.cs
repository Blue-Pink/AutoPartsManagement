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
    public class InboundOrderService(IConTaxiService taxi) : IInboundOrderService
    {
        public PagingData<InboundOrder> GetInboundOrders(int pageIndex = 1, int pageSize = 10)
        {
            var list = taxi.GetDataSetQuery<InboundOrder>(pageIndex: pageIndex, pageSize: pageSize).ToList();
            var total = taxi.Total<InboundOrder>();
            return new PagingData<InboundOrder>(list, total, pageIndex, pageSize);
        }

        public InboundOrder? GetInboundOrder(Guid id)
        {
            if (id == Guid.Empty) return null;
            var order = taxi.Get<InboundOrder>(id);
            if (order == null) return null;

            // 尝试加载明细并赋值（若实体包含集合属性）
            var items = taxi.GetDataSetQuery<InboundItem>(i => i.InboundOrderId == id, paging: false).ToList();
            var prop = order.GetType().GetProperty("InboundItems");
            if (prop != null && prop.PropertyType.IsAssignableFrom(typeof(List<InboundItem>)))
            {
                prop.SetValue(order, items);
            }
            return order;
        }

        public InboundOrder EditInboundOrder(InboundOrderDTO dto)
        {
            if (dto is null)
                throw new APMException("参数 dto 不可为空");

            // 计算/校验合计金额
            var computedTotal = dto.Items?.Sum(i => i.Price * i.Quantity) ?? 0m;

            if (dto.Id is null || dto.Id == Guid.Empty || taxi.Get<InboundOrder>(dto.Id.Value) == null)
            {
                // 新增入库单（先创建主表）
                var order = new InboundOrder
                {
                    OrderNo = dto.OrderNo ?? string.Empty,
                    SupplierId = dto.SupplierId,
                    OperatorUserId = dto.OperatorUserId,
                    Remark = dto.Remark,
                    TotalAmount = computedTotal
                };

                var created = taxi.Create(order);

                // 创建明细
                if (dto.Items != null && dto.Items.Any())
                {
                    var items = dto.Items.Select(it => new InboundItem
                    {
                        PartId = it.PartId,
                        Quantity = it.Quantity,
                        Price = it.Price,
                        TotalAmount = it.Price * it.Quantity,
                        InboundOrderId = created.Id
                    }).ToList();

                    taxi.Create(items);
                }

                // 返回包含主键的实体（如果需要包含明细可再次查询）
                return taxi.Get<InboundOrder>(created.Id) ?? throw new APMException("创建入库单失败");
            }

            // 更新
            var existing = taxi.Get<InboundOrder>(dto.Id.Value) ?? throw new APMException("要更新的入库单不存在");
            existing.OrderNo = dto.OrderNo ?? existing.OrderNo;
            existing.SupplierId = dto.SupplierId;
            existing.OperatorUserId = dto.OperatorUserId;
            existing.Remark = dto.Remark;
            existing.TotalAmount = computedTotal;

            var updatedOrder = taxi.Update(existing);

            // 处理明细：删除旧明细并新增新明细（简化实现）
            taxi.Delete<InboundItem>(i => i.InboundOrderId == updatedOrder.Id);

            if (dto.Items != null && dto.Items.Any())
            {
                var items = dto.Items.Select(it => new InboundItem
                {
                    PartId = it.PartId,
                    Quantity = it.Quantity,
                    Price = it.Price,
                    TotalAmount = it.Price * it.Quantity,
                    InboundOrderId = updatedOrder.Id
                }).ToList();

                taxi.Create(items);
            }

            return taxi.Get<InboundOrder>(updatedOrder.Id) ?? throw new APMException("更新后读取入库单失败");
        }

    }
}