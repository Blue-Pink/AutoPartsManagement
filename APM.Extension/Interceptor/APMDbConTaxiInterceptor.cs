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

            SaveInboundItemParallel(context);

            UpdateBaseEntityField(context);

            SaveOutboundItemParallel(context);

            SavePartParallel(context);

            return base.SavingChanges(eventData, result);
        }

        private static IEnumerable<EntityEntry<T>> GetEntries<T>(DbContext context, List<EntityState> status) where T : BaseEntity
        {
            return context.ChangeTracker.Entries<T>()
                .Where(e => status.Contains(e.State));
        }

        private static IEnumerable<T> GetEntities<T>(DbContext context, List<EntityState> status) where T : BaseEntity
        {
            return context.ChangeTracker.Entries<T>()
                .Where(e => status.Contains(e.State))
                .Select(e => e.Entity);
        }

        private static void SaveInboundItemParallel(DbContext context)
        {
            var addItems = GetEntities<InboundItem>(context, [EntityState.Added]);
            var partIds = addItems.Select(ai => ai.PartId).ToList();
            var exitPartItem = context.Set<InboundItem>().Count(i => partIds.Contains(i.PartId));
            if (exitPartItem > 0)
                throw new APMException($"明细中已存在同个配件");

            var changedItemsEntries = GetEntries<InboundItem>(context,
                [EntityState.Added, EntityState.Modified, EntityState.Deleted]);
            var editItems = GetEntities<InboundItem>(context, [EntityState.Added, EntityState.Modified]);
            var deleteItems = GetEntities<InboundItem>(context, [EntityState.Deleted]);

            //先计算明细的总金额
            foreach (var item in editItems)
            {
                item.TotalAmount = item.Quantity * item.Price;
            }

            var orderIds = changedItemsEntries.Select(e => e.Entity).Select(i => i.InboundOrderId).Distinct();
            var orders = context.Set<InboundOrder>().Where(order => orderIds.Contains(order.Id)).ToList();

            //查询相关出库单的明细(不再获取此次更新与删除的明细)
            var items = context.Set<InboundItem>()
                .Where(i => orderIds.Contains(i.InboundOrderId)
                            && !editItems.Select(ei => ei.Id).Contains(i.Id)
                            && !deleteItems.Select(ei => ei.Id).Contains(i.Id))
                .Select(i => new { i.TotalAmount, i.InboundOrderId })
                .ToList();

            foreach (var order in orders)
            {
                //入库单总金额: 查询到的明细总金额 + 此次更新的明细总金额
                order.TotalAmount = items.Where(i => i.InboundOrderId == order.Id).Sum(i => i.TotalAmount)
                                    + editItems.Where(i => i.InboundOrderId == order.Id).Sum(i => i.TotalAmount);
                context.Update(order);
            }

            //计算库存
            var parts = context.Set<Part>().Where(p => changedItemsEntries.Select(i => i.Entity.PartId).Contains(p.Id)).ToList();
            foreach (var entry in changedItemsEntries)
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
                                throw new Exception($"明细变更后 [{part.PartName}({part.Model})] - [{part.OECode}] 库存不足");

                            part.Stockpiles += delta;
                            break;
                        }
                    case EntityState.Deleted:
                        part.Stockpiles -= entry.Entity.Quantity;
                        break;
                }
            }
        }

        private static void SaveOutboundItemParallel(DbContext context)
        {
            var addItems = GetEntities<OutboundItem>(context, [EntityState.Added]);
            var partIds = addItems.Select(ai => ai.PartId).ToList();
            var exitPartItem = context.Set<OutboundItem>().Count(i => partIds.Contains(i.PartId));
            if (exitPartItem > 0)
                throw new APMException($"明细中已存在同个配件");
            var changedItemsEntries = GetEntries<OutboundItem>(context,
                [EntityState.Added, EntityState.Modified, EntityState.Deleted]);
            var editItems = GetEntities<OutboundItem>(context, [EntityState.Added, EntityState.Modified]);
            var deleteItems = GetEntities<OutboundItem>(context, [EntityState.Deleted]);

            //先计算明细的总金额
            foreach (var item in editItems)
            {
                item.TotalAmount = item.Quantity * item.Price;
            }

            var orderIds = changedItemsEntries.Select(e => e.Entity).Select(i => i.OutboundOrderId).Distinct();
            var orders = context.Set<OutboundOrder>().Where(order => orderIds.Contains(order.Id)).ToList();

            //查询相关出库单的明细(不再获取此次更新与删除的明细)
            var items = context.Set<OutboundItem>()
                .Where(i => orderIds.Contains(i.OutboundOrderId)
                            && !editItems.Select(ei => ei.Id).Contains(i.Id)
                            && !deleteItems.Select(ei => ei.Id).Contains(i.Id))
                .Select(i => new { i.TotalAmount, i.OutboundOrderId })
                .ToList();

            foreach (var order in orders)
            {
                //入库单总金额: 查询到的明细总金额 + 此次更新的明细总金额
                order.TotalAmount = items.Where(i => i.OutboundOrderId == order.Id).Sum(i => i.TotalAmount)
                                    + editItems.Where(i => i.OutboundOrderId == order.Id).Sum(i => i.TotalAmount);
                context.Update(order);
            }

            //计算库存
            var parts = context.Set<Part>().Where(p => changedItemsEntries.Select(i => i.Entity.PartId).Contains(p.Id)).ToList();
            foreach (var entry in changedItemsEntries.Where(a => a.State is EntityState.Added or EntityState.Modified))
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
            }
        }

        private static void SavePartParallel(DbContext context)
        {
            var parts = GetEntries<Part>(context, [EntityState.Added, EntityState.Modified]);
            foreach (var entry in parts)
            {
                var originStock = Convert.ToInt32(entry.OriginalValues["Stockpiles"]);
                var part = entry.Entity;
                if ((part.Stockpiles < part.MinStock && part.Stockpiles < originStock) || (part.Stockpiles > part.MaxStock && part.Stockpiles > originStock))
                    throw new APMException(
                        $"配件 [{part.PartName}({part.Model})] - [{part.OECode}] - [最大:{part.MaxStock},最小:{part.MinStock}] 库存{((part.Stockpiles < part.MinStock && part.Stockpiles < originStock) ? "不足" : "超出")}");
            }
        }

        private void UpdateBaseEntityField(DbContext context)
        {
            var changedEntries = GetEntries<BaseEntity>(context, [EntityState.Added, EntityState.Modified]);

            foreach (var entry in changedEntries)
            {
                entry.Entity.OperatorUserId = userContext.UserId ?? new Guid(ConstDictionary.AdministratorId);
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.Id = entry.Entity.Id == Guid.Empty ? Guid.NewGuid() : entry.Entity.Id;
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                        entry.Entity.ModifiedAt = DateTime.UtcNow;
                        break;
                    case EntityState.Modified:
                        entry.Entity.ModifiedAt = DateTime.UtcNow;
                        break;
                }
            }
        }
    }
}
