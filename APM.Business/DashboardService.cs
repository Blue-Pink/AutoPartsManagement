using APM.DbEntities.DTOs;
using APM.DbEntities.Views;
using APM.IBusiness;
using System;
using System.Collections.Generic;
using System.Text;
using APM.ConTaxi.Taxi;
using Microsoft.EntityFrameworkCore;

namespace APM.Business
{
    public class DashboardService(IConTaxiService taxi) : IDashboardService
    {
        public DashboardDTO GetStockFlowDash()
        {
            var today = DateTime.Today;
            var todayData = taxi.GetDataSetQuery<AllBoundItemView>()
                .Where(x => x.CreatedAt >= today)
                .Include(v => v.Part)
                .ThenInclude(p => p!.Category)
                .AsEnumerable();
            var dto = new DashboardDTO
            {
                TodayInboundQuantity = todayData.Where(v => v.Type == 1).Sum(x => x.Quantity),
                TodayInboundTotalAmount = todayData.Where(v => v.Type == 1).Sum(x => x.TotalAmount),
                TodayOutboundQuantity = todayData.Where(v => v.Type == 0).Sum(x => x.Quantity),
                TodayOutboundTotalAmount = todayData.Where(v => v.Type == 0).Sum(x => x.TotalAmount),

                // 分类占比 (饼图)
                CategoryDistribution = todayData
                    .GroupBy(x => x.Part?.Category?.Name)
                    .Select(v => new PieChartData(v.Key ?? "未知",
                        v.Where(g => g.Type == 1).Sum(vi => vi.Quantity),
                        v.Where(g => g.Type == 1).Sum(vi => vi.TotalAmount),
                        v.Where(g => g.Type == 0).Sum(vi => vi.Quantity),
                        v.Where(g => g.Type == 0).Sum(vi => vi.TotalAmount))
                        )
                    .ToList()
            };


            // 近七天趋势
            var aWeekData = taxi.GetDataSetQuery<AllBoundItemView>()
                .Where(v => v.CreatedAt.HasValue && v.CreatedAt >= today.AddDays(-7))
                .GroupBy(v => v.CreatedAt!.Value.Month + "-" + v.CreatedAt.Value.Day)
                .Select(v => new
                {
                    Date = v.Key,
                    InboundQuantity = v.Where(g => g.Type == 1).Sum(vi => vi.Quantity),
                    InboundTotalAmount = v.Where(g => g.Type == 1).Sum(vi => vi.TotalAmount),
                    OutboundQuantity = v.Where(g => g.Type == 0).Sum(vi => vi.Quantity),
                    OutboundTotalAmount = v.Where(g => g.Type == 0).Sum(vi => vi.TotalAmount),
                })
                .AsEnumerable();

            foreach (var grouping in aWeekData)
            {
                dto.LastSevenDaysTrend.Add(new LineChartData(
                    grouping.Date,
                    grouping.InboundQuantity,
                    grouping.InboundTotalAmount,
                    grouping.OutboundQuantity,
                    grouping.OutboundTotalAmount
                ));
            }

            return dto;
        }
    }
}
