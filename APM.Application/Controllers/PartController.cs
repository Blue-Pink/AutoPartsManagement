using APM.DbEntities;
using APM.DbEntities.DTOs;
using APM.DbEntities.Views;
using APM.IBusiness;
using APM.UtilEntities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace APM.Application.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class PartController(IPartService partService) : APMController
    {
        [HttpGet, Route("[action]")]
        public UsualApiData<PartView> GetParts(int pageIndex, int pageSize, string sortField = "", bool sortDesc = false)
        {
            return UsualResult(partService.GetParts(pageIndex, pageSize, sortField, sortDesc));
        }

        /// <summary>
        /// 更新单个零件信息
        /// </summary>
        [HttpPost, Route("[action]")]
        public UsualApiData<Part?> EditPart([FromBody] PartDTO partDTO)
        {
            if (partDTO is null)
                throw new APMException("参数 partDTO 不可为空");

            var updated = partService.EditPart(partDTO);
            return UsualResult(updated);
        }

        /// <summary>
        /// 获取所有分类
        /// </summary>
        [HttpGet, Route("[action]")]
        public UsualApiData<PartCategory> GetCategories()
        {
            var categories = partService.GetCategories();
            return UsualResult(categories);
        }

        /// <summary>
        /// 获取所有单位
        /// </summary>
        [HttpGet, Route("[action]")]
        public UsualApiData<PartUnit> GetUnits()
        {
            var units = partService.GetUnits();
            return UsualResult(units);
        }
    }
}
