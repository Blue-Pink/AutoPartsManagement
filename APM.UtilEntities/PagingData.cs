using System;
using System.Collections.Generic;
using System.Text;

namespace APM.UtilEntities
{
    public class PagingData<T>(IEnumerable<T> data, int total, int pageIndex, int pageSize)
    {
        public IEnumerable<T> Data { get; set; } = data;
        public int Total { get; set; } = total;
        public int PageSize { get; set; } = pageSize;
        public int PageIndex { get; set; } = pageIndex;
    }
}
