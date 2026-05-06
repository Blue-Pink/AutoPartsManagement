using APM.DbEntities;
using APM.DbEntities.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using APM.IServices;
using APM.UtilEntities;

namespace APM.Extensions.Interceptor
{
    public class APMDbConTaxiInterceptor(IUserContext userContext) : SaveChangesInterceptor
    {
        // 重写同步保存前的拦截方法
        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            var context = eventData.Context;
            if (context == null) return result;

            SaveInboundItemTotalCalAmount(context);

            UpdateOperatorUserId(context);

            return base.SavingChanges(eventData, result);
        }

        private static void SaveInboundItemTotalCalAmount(DbContext context)
        {
            var changedItems = context.ChangeTracker.Entries<InboundItem>()
                .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
                .ToList();

            var editItems = changedItems.Where(e => e.State is EntityState.Added or EntityState.Modified)
                .Select(e => e.Entity)
                .ToList();

            var deleteItems = changedItems.Where(e => e.State is EntityState.Deleted)
                .Select(e => e.Entity)
                .ToList();


            foreach (var item in editItems)
            {
                item.TotalAmount = item.Quantity * item.Price;
            }

            var orderIds = changedItems.Select(e => e.Entity).Select(i => i.InboundOrderId).Distinct();

            var items = context.Set<InboundItem>()
                .Where(i => orderIds.Contains(i.InboundOrderId)
                            && !editItems.Select(ei => ei.Id).Contains(i.Id)
                            && !deleteItems.Select(ei => ei.Id).Contains(i.Id))
                .Select(i => new { i.TotalAmount, i.InboundOrderId })
                .ToList();

            foreach (var orderId in orderIds)
            {
                var order = context.Set<InboundOrder>().Find(orderId);
                if (order is null)
                    throw new APMException($"数据 {nameof(InboundOrder)} - [{orderId}] 丢失");

                order.TotalAmount = items.Where(i => i.InboundOrderId == order.Id).Sum(i => i.TotalAmount)
                                    + editItems.Where(i => i.InboundOrderId == order.Id).Sum(i => i.TotalAmount);
                context.Update(order);
            }

        }

        private void UpdateOperatorUserId(DbContext context)
        {
            var changedEntries = context.ChangeTracker.Entries<BaseEntity>()
                .Where(e => e.State is EntityState.Added or EntityState.Modified);

            foreach (var entry in changedEntries)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.Id = entry.Entity.Id == Guid.Empty ? Guid.NewGuid() : entry.Entity.Id;
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                        break;
                    case EntityState.Modified:
                        entry.Entity.ModifiedAt = DateTime.UtcNow;
                        break;
                }
            }
        }
    }
}
