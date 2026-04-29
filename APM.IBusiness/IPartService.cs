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
        /// <summary>
        /// 获取配件列表
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sortField"></param>
        /// <param name="sortDesc"></param>
        /// <returns></returns>
        public PagingData<PartView> GetParts(int pageIndex, int pageSize, string sortField = "", bool sortDesc = false);
        /// <summary>
        /// 编辑(新增/更新)配件
        /// </summary>
        /// <param name="partDTO">更新时指定配件Id</param>
        /// <returns></returns>
        Part EditPart(PartDTO partDTO);
        /// <summary>
        /// 获取所有配件分类
        /// </summary>
        /// <returns></returns>
        IEnumerable<PartCategory> GetCategories();
        /// <summary>
        /// 获取所有单位
        /// </summary>
        /// <returns></returns>
        IEnumerable<PartUnit> GetUnits();
    }
}
