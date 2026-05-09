using APM.DbEntities;
using APM.DbEntities.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using APM.IServices;
using APM.UtilEntities;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace APM.Extensions.Interceptor
{
    public class APMDbConTaxiInterceptor(IUserContext userContext) : SaveChangesInterceptor
    {
        // 重写同步保存前的拦截方法
        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            var context = eventData.Context;
            if (context == null) return result;

            UpdateBaseEntityField(context);

            SaveInboundOrderParallel(context);

            SaveInboundItemParallel(context);

            SaveOutboundOrderParallel(context);

            SaveOutboundItemParallel(context);

            SavePartParallel(context);

            return base.SavingChanges(eventData, result);
        }

        private static IEnumerable<EntityEntry<T>> GetEntries<T>(DbContext context, List<EntityState>? status = null) where T : BaseEntity
        {
            return context.ChangeTracker.Entries<T>()
                .Where(e => status is null || status.Contains(e.State));
        }

        private static IEnumerable<T> GetEntities<T>(DbContext context, List<EntityState>? status = null) where T : BaseEntity
        {
            return context.ChangeTracker.Entries<T>()
                .Where(e => status is null || status.Contains(e.State))
                .Select(e => e.Entity);
        }

        private static void SaveInboundItemParallel(DbContext context)
        {
            var editItems = GetEntries<InboundItem>(context, [EntityState.Added, EntityState.Modified])
                .Select(e => e.Entity);
            if (editItems.Any())
            {
                var modifiedItemIds = editItems.Select(mi => mi.Id);
                var modifiedOrderIds = editItems.Select(mi => mi.InboundOrderId);
                var modifiedPartIds = editItems.Select(mi => mi.PartId);
                //当前更新的所有明细的同父级其他明细
                var otherItems = context.Set<InboundItem>()
                    .Where(oi =>
                        modifiedOrderIds.Contains(oi.InboundOrderId)
                        && modifiedPartIds.Contains(oi.PartId)
                        && !modifiedItemIds.Contains(oi.Id))
                    .Include(oi => oi.Part)
                    .Include(oi => oi.InboundOrder)
                    .Select(oi => new { oi.InboundOrderId, oi.PartId, oi.Id, oi.Part, oi.InboundOrder })
                    .ToList();

                //当其他明细有同主单据同配件时阻止更新
                foreach (var otherItem in otherItems)
                {
                    if (editItems.Any(editItem =>
                            otherItem.InboundOrderId == editItem.InboundOrderId &&
                            otherItem.PartId == editItem.PartId))
                        throw new APMException(
                            $"入库单 [{otherItem.InboundOrder?.OrderNo}] 中存在同种配件 [{otherItem.Part?.PartName} ({otherItem.Part?.Model})] 的明细");
                }
            }

            //先计算明细的总金额
            foreach (var item in editItems)
            {
                item.TotalAmount = item.Quantity * item.Price;
            }

            //查询相关出库单的明细(不再获取此次更新与删除的明细)
            var changedItemsEntries = GetEntries<InboundItem>(context,
                [EntityState.Added, EntityState.Modified, EntityState.Deleted]);
            var deleteItems = GetEntities<InboundItem>(context, [EntityState.Deleted]);
            var orderIds = changedItemsEntries.Where(e => e.State != EntityState.Deleted)
                .Select(e => e.Entity).Select(i => i.InboundOrderId).Distinct();
            var items = context.Set<InboundItem>()
                .Where(i => orderIds.Contains(i.InboundOrderId)
                            && !editItems.Select(ei => ei.Id).Contains(i.Id)
                            && !deleteItems.Select(ei => ei.Id).Contains(i.Id))
                .Select(i => new { i.TotalAmount, i.InboundOrderId })
                .ToList();
            var orders = context.Set<InboundOrder>().Where(order => orderIds.Contains(order.Id)).ToList();
            foreach (var order in orders)
            {
                //入库单总金额: 查询到的明细总金额 + 此次更新的明细总金额
                order.TotalAmount = items.Where(i => i.InboundOrderId == order.Id).Sum(i => i.TotalAmount)
                                    + editItems.Where(i => i.InboundOrderId == order.Id).Sum(i => i.TotalAmount);
                context.Entry(order).State = EntityState.Modified;
            }

            //更新配件库存数据
            var parts = context.Set<Part>().Where(p => changedItemsEntries.Select(i => i.Entity.PartId).Contains(p.Id))
                .ToList();

            foreach (var entry in changedItemsEntries.ToList())
            {
                var part = parts.First(p => p.Id == entry.Entity.PartId);
                switch (entry.State)
                {
                    case EntityState.Added:
                        part.Stockpiles += entry.Entity.Quantity;
                        break;
                    case EntityState.Modified:
                        {
                            // 1. 获取差额：新值 - 旧值
                            var currentQty = entry.Entity.Quantity;
                            var originalQty = (int)entry.OriginalValues["Quantity"]!;
                            var delta = currentQty - originalQty;

                            if (delta == 0) continue; // 数量没变，不操作库存

                            // 出库数量增加，意味着物理库存要减少，所以用减法
                            if (part.Stockpiles < delta)
                                throw new APMException($"明细变更后 [{part.PartName}({part.Model})] - [{part.OECode}] 库存不足");

                            part.Stockpiles += delta;
                            break;
                        }
                    case EntityState.Deleted:
                        part.Stockpiles -= entry.Entity.Quantity;
                        break;
                }
                context.Entry(part).State = EntityState.Modified;
            }

        }

        private static void SaveOutboundItemParallel(DbContext context)
        {
            var editItems = GetEntries<OutboundItem>(context, [EntityState.Added, EntityState.Modified])
                .Select(e => e.Entity);
            if (editItems.Any())
            {
                var modifiedItemIds = editItems.Select(mi => mi.Id);
                var modifiedOrderIds = editItems.Select(mi => mi.OutboundOrderId);
                var modifiedPartIds = editItems.Select(mi => mi.PartId);
                //当前更新的所有明细的同父级其他明细
                var otherItems = context.Set<OutboundItem>()
                    .Where(oi => modifiedOrderIds.Contains(oi.OutboundOrderId)
                                 && modifiedPartIds.Contains(oi.PartId)
                                 && !modifiedItemIds.Contains(oi.Id))
                    .Include(oi => oi.Part)
                    .Include(oi => oi.OutboundOrder)
                    .Select(oi => new { oi.OutboundOrderId, oi.PartId, oi.Id, oi.Part, oi.OutboundOrder })
                    .ToList();

                //当其他明细有同主单据同配件时阻止更新
                foreach (var otherItem in otherItems)
                {
                    if (editItems.Any(editItem => otherItem.OutboundOrderId == editItem.OutboundOrderId && otherItem.PartId == editItem.PartId))
                        throw new APMException($"出库单 [{otherItem.OutboundOrder?.OrderNo}] 中存在同种配件 [{otherItem.Part?.PartName} ({otherItem.Part?.Model})] 的明细");
                }
            }

            //先计算明细的总金额
            foreach (var item in editItems)
            {
                item.TotalAmount = item.Quantity * item.Price;
            }

            //查询相关出库单的明细(不再获取此次更新与删除的明细)
            var changedItemsEntries = GetEntries<OutboundItem>(context,
                [EntityState.Added, EntityState.Modified, EntityState.Deleted]);
            var deleteItems = GetEntities<OutboundItem>(context, [EntityState.Deleted]);
            var orderIds = changedItemsEntries.Where(e => e.State != EntityState.Deleted)
                .Select(e => e.Entity).Select(i => i.OutboundOrderId).Distinct();
            var items = context.Set<OutboundItem>()
                .Where(i => orderIds.Contains(i.OutboundOrderId)
                            && !editItems.Select(ei => ei.Id).Contains(i.Id)
                            && !deleteItems.Select(ei => ei.Id).Contains(i.Id))
                .Select(i => new { i.TotalAmount, i.OutboundOrderId })
                .ToList();
            var orders = context.Set<OutboundOrder>().Where(order => orderIds.Contains(order.Id)).ToList();
            foreach (var order in orders)
            {
                //入库单总金额: 查询到的明细总金额 + 此次更新的明细总金额
                order.TotalAmount = items.Where(i => i.OutboundOrderId == order.Id).Sum(i => i.TotalAmount)
                                    + editItems.Where(i => i.OutboundOrderId == order.Id).Sum(i => i.TotalAmount);
                context.Entry(order).State = EntityState.Modified;
            }

            //更新配件库存数据
            var parts = context.Set<Part>().Where(p => changedItemsEntries.Select(i => i.Entity.PartId).Contains(p.Id)).ToList();

            foreach (var entry in changedItemsEntries.ToList())
            {
                var part = parts.First(p => p.Id == entry.Entity.PartId);
                switch (entry.State)
                {
                    case EntityState.Added:
                        part.Stockpiles -= entry.Entity.Quantity;
                        break;
                    case EntityState.Modified:
                        {
                            // 1. 获取差额：新值 - 旧值
                            var currentQty = entry.Entity.Quantity;
                            var originalQty = (int)entry.OriginalValues["Quantity"]!;
                            var delta = currentQty - originalQty;

                            if (delta == 0) continue; // 数量没变，不操作库存

                            // 出库数量增加，意味着物理库存要减少，所以用减法
                            if (part.Stockpiles < delta)
                                throw new APMException($"明细变更后 [{part.PartName}({part.Model})] - [{part.OECode}] 库存不足");

                            part.Stockpiles -= delta;
                            break;
                        }
                    case EntityState.Deleted:
                        part.Stockpiles += entry.Entity.Quantity;
                        break;
                }
                context.Entry(part).State = EntityState.Modified;
            }
        }

        private static void SavePartParallel(DbContext context)
        {
            var parts = GetEntries<Part>(context, [EntityState.Added, EntityState.Modified]);
            foreach (var entry in parts)
            {
                var part = entry.Entity;

                if (part.Stockpiles <= 0)
                    throw new APMException(
                        $"配件 [{part.PartName}({part.Model})] - [{part.OECode}] - [最大:{part.MaxStock},最小:{part.MinStock}] 库存不足");

                //var originStock = Convert.ToInt32(entry.OriginalValues["Stockpiles"]);
                //if ((part.Stockpiles < part.MinStock && part.Stockpiles < originStock) || (part.Stockpiles > part.MaxStock && part.Stockpiles > originStock))
                //    throw new APMException(
                //        $"配件 [{part.PartName}({part.Model})] - [{part.OECode}] - [最大:{part.MaxStock},最小:{part.MinStock}] 库存{((part.Stockpiles < part.MinStock && part.Stockpiles < originStock) ? "不足" : "超出")}");
            }
        }

        private static void SaveInboundOrderParallel(DbContext context)
        {
            var orders = GetEntities<InboundOrder>(context, [EntityState.Deleted]);
            if (!orders.Any())
                return;
            var items = context.Set<InboundItem>().Where(i => orders.Select(o => o.Id).Contains(i.InboundOrderId)).ToList();
            foreach (var inboundItem in items)
            {
                context.Entry(inboundItem).State = EntityState.Deleted;
            }
        }

        private static void SaveOutboundOrderParallel(DbContext context)
        {
            var orders = GetEntities<OutboundOrder>(context, [EntityState.Deleted]);
            if (!orders.Any())
                return;
            var items = context.Set<OutboundItem>().Where(i => orders.Select(o => o.Id).Contains(i.OutboundOrderId)).ToList();
            foreach (var outboundItem in items)
            {
                context.Entry(outboundItem).State = EntityState.Deleted;
            }
        }

        private void UpdateBaseEntityField(DbContext context)
        {
            var changedEntries = GetEntries<BaseEntity>(context, [EntityState.Added, EntityState.Modified]);
            var now = DateTime.UtcNow;
            foreach (var entry in changedEntries)
            {
                entry.Entity.OperatorUserId = userContext.UserId ?? new Guid(ConstDictionary.AdministratorId);
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.Id = entry.Entity.Id == Guid.Empty ? Guid.NewGuid() : entry.Entity.Id;
                        entry.Entity.CreatedAt = now;
                        entry.Entity.ModifiedAt = now;
                        break;
                    case EntityState.Modified:
                        entry.Entity.ModifiedAt = now;
                        break;
                }
            }
        }
    }
}
