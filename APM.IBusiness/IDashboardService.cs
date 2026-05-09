using System;
using System.Collections.Generic;
using System.Text;
using APM.DbEntities.DTOs;

namespace APM.IBusiness
{
    public interface IDashboardService
    {
        public DashboardDTO GetStockFlowDash();
    }
}
