using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using APM.ConTaxi.Taxi;
using APM.DbEntities;
using APM.DbEntities.DTOs;
using APM.DbEntities.Views;
using APM.IBusiness;
using APM.UtilEntities;

namespace APM.Business
{
    public class PartService(IConTaxiService taxi) : IPartService
    {
        public PagingData<PartView> GetParts(int pageIndex, int pageSize, string sortField = "", bool sortDesc = false)
        {
            Expression<Func<PartView, object?>>? orderBy = null;
            if (!string.IsNullOrEmpty(sortField))
            {
                orderBy = sortField switch
                {
                    "partName" => x => x.PartName,
                    "createdAt" => x => x.CreatedAt,
                    "modifiedAt" => x => x.ModifiedAt,
                    "maxStock" => x => x.MaxStock,
                    "minStock" => x => x.MinStock,
                    "costPrice" => x => x.CostPrice,
                    "sellingPrice" => x => x.SellingPrice,
                    "oeCode" => x => x.OECode,
                    _ => null
                };
            }
            var query = taxi.GetDataSetQuery(pageIndex: pageIndex, pageSize: pageSize, orderBy: orderBy, descending: sortDesc);
            var parts = query.ToList();
            var total = taxi.Total<PartView>();
            return new PagingData<PartView>(parts, total, pageIndex, pageSize);
        }

        public Part EditPart(PartDTO partDTO)
        {
            if (partDTO is null)
                throw new APMException("参数 part 不可为空");

            const decimal minPrice = (decimal)0.01;

            if (partDTO.PartName.Length is <= 2 or > 50
                || partDTO.OECode.Length is <= 2 or > 50
                || string.IsNullOrEmpty(partDTO.Model)
                || partDTO.Model.Length is <= 2 or > 50
                || string.IsNullOrEmpty(partDTO.Brand)
                || partDTO.Brand.Length is <= 2 or > 50)
                throw new APMException($"名称,OE代码,型号,品牌长度控制在2-50");

            if (partDTO.CostPrice < minPrice || partDTO.SellingPrice < minPrice)
                throw new APMException($"成本价与售价不可低于{minPrice}");

            var unit = taxi.Get<PartUnit>(partDTO.UnitId);
            var category = taxi.Get<PartCategory>(partDTO.CategoryId);
            if (unit is null || category is null)
                throw new APMException("选择了无效的单位或分类");

            if (partDTO.Id == Guid.Empty || partDTO.Id == null || taxi.Get<Part>(partDTO.Id.Value) == null)
            {
                if (taxi.FirstOrDefault<Part>(p =>
                        p.OECode.ToLower() == partDTO.OECode.ToLower()) is null)
                    return taxi.Create(new Part
                    {
                        PartName = partDTO.PartName,
                        OECode = partDTO.OECode,
                        Brand = partDTO.Brand,
                        CategoryId = partDTO.CategoryId,
                        UnitId = partDTO.UnitId,
                        Model = partDTO.Model,
                        CostPrice = partDTO.CostPrice,
                        MaxStock = partDTO.MaxStock,
                        MinStock = partDTO.MinStock,
                        Remark = partDTO.Remark,
                        SellingPrice = partDTO.SellingPrice,
                    });
                throw new APMException($"OE代码 {partDTO.OECode} 已录入, 不可重复录入");
            }

            var part = taxi.Get<Part>(partDTO.Id.Value);
            if (part is null)
                throw new APMException($"未找到该配件");

            part.PartName = partDTO.PartName;
            part.OECode = partDTO.OECode;
            part.Brand = partDTO.Brand;
            part.CategoryId = partDTO.CategoryId;
            part.UnitId = partDTO.UnitId;
            part.Model = partDTO.Model;
            part.CostPrice = partDTO.CostPrice;
            part.MaxStock = partDTO.MaxStock;
            part.MinStock = partDTO.MinStock;
            part.Remark = partDTO.Remark;
            part.SellingPrice = partDTO.SellingPrice;

            return taxi.Update(part);

        }

        /// <summary>
        /// 返回所有分类数据
        /// </summary>
        public IEnumerable<PartCategory> GetCategories()
        {
            return taxi.GetDataSetQuery<PartCategory>(paging: false).ToList();
        }

        /// <summary>
        /// 返回所有单位数据
        /// </summary>
        public IEnumerable<PartUnit> GetUnits()
        {
            return taxi.GetDataSetQuery<PartUnit>(paging: false).ToList();
        }
    }
}
