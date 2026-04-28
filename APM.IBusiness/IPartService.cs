using System;
using System.Collections.Generic;
using System.Text;
using APM.DbEntities;
using APM.DbEntities.DTOs;
using APM.DbEntities.Views;
using APM.UtilEntities;

namespace APM.IBusiness
{
    public interface IPartService
    {
        public PagingData<PartView> GetParts(int pageIndex, int pageSize, string sortField = "", bool sortDesc = false);
        int DeleteParts(IEnumerable<Guid> ids);
        Part EditPart(PartDTO partDTO);
        IEnumerable<Category> GetCategories();
        IEnumerable<Unit> GetUnits();
    }
}
    