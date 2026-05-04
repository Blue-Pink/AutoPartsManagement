using APM.DbEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Text;
using APM.UtilEntities;

namespace APM.Extensions.Interceptor
{
    public class CalculatorInterceptor : SaveChangesInterceptor
    {
        // 重写同步保存前的拦截方法
        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            var context = eventData.Context;
            if (context == null) return result;

            SaveInboundItemTotalCalAmount(context);

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
    }
}
