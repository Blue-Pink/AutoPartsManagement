using System;
using System.Collections.Generic;
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
        public PagingData<PartView> GetParts(int pageIndex, int pageSize)
        {
            var parts = taxi.GetDataSetQuery<PartView>(pageIndex: pageIndex, pageSize: pageSize).ToList();
            var total = taxi.Total<PartView>();
            return new PagingData<PartView>(parts, total, pageIndex, pageSize);
        }

        public int DeleteParts(IEnumerable<Guid> ids)
        {
            return taxi.Delete<Part>(ids);
        }

        public Part EditPart(PartDTO partDTO)
        {
            if (partDTO is null)
                throw new APMException("参数 part 不可为空");

            if (partDTO.Id == Guid.Empty || partDTO.Id == null || taxi.Get<Part>(partDTO.Id.Value) == null)
            {
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
        public IEnumerable<Category> GetCategories()
        {
            return taxi.GetDataSetQuery<Category>(paging: false).ToList();
        }

        /// <summary>
        /// 返回所有单位数据
        /// </summary>
        public IEnumerable<Unit> GetUnits()
        {
            return taxi.GetDataSetQuery<Unit>(paging: false).ToList();
        }
    }
}
