using System;
using System.Collections.Generic;
using System.Text;

namespace APM.DbEntities.DTOs
{
    public class DashboardDTO
    {
        public int TodayInboundQuantity { get; set; }
        public decimal TodayInboundTotalAmount { get; set; }
        public decimal TodayOutboundQuantity { get; set; }
        public decimal TodayOutboundTotalAmount { get; set; }
        public List<PieChartData> CategoryDistribution { get; set; } = [];
        public List<LineChartData> LastSevenDaysTrend { get; set; } = [];
    }

    public record PieChartData(string CategoryName, decimal InboundQuantity, decimal InboundTotalAmount, decimal OutboundQuantity, decimal OutboundTotalAmount);
    public record LineChartData(string Date, decimal InboundQuantity,decimal InboundTotalAmount, decimal OutboundQuantity, decimal OutboundTotalAmount);
}
