using APM.ConTaxi.Taxi;
using APM.DbEntities;
using APM.IServices;
using APM.Services;
using APM.UtilEntities;
using Bogus;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace APM.Application.Controllers
{
    [ApiController, Route("api/[controller]"), AllowAnonymous]
    public class TestController(
        IJsonWebTokenService tokenService,
        IConTaxiService taxi,
        IHttpContextAccessor httpContextAccessor)
        : APMController
    {
        [HttpGet, Route("[action]")]
        public string Test0()
        {
            var claim = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
            var userid = claim != null ? Guid.Parse(claim.Value) : Guid.Empty;
            return userid.ToString();
            //return BCrypt.Net.BCrypt.HashPassword(@$"Administrator@01", BCrypt.Net.BCrypt.GenerateSalt());
        }

        [HttpGet, Route("[action]"), Authorize(AuthenticationSchemes = ConstDictionary.Bearer)]
        public UsualApiData<Part?> Test1()
        {
            //var categories = new List<Category>()
            //{
            //    new Category()
            //    {
            //        Name = "发动机系统",
            //        Description = "发动机系统",
            //    },
            //    new Category()
            //    {
            //        Name = "制动系统",
            //        Description = "制动系统",
            //    },
            //    new Category()
            //    {
            //        Name = "悬挂系统",
            //        Description = "悬挂系统",
            //    },
            //    new Category()
            //    {
            //        Name = "外饰",
            //        Description = "外饰",
            //    },

            //};
            //var units = new List<Unit>()
            //{
            //    new Unit()
            //    {
            //        Name = "个",
            //    },
            //    new Unit()
            //    {
            //        Name = "片",
            //    },
            //    new Unit()
            //    {
            //        Name = "台",
            //    },
            //};
            //taxi.Transaction(categories, EntityState.Added);
            //taxi.Transaction(units, EntityState.Added);

            var categories = taxi.GetDataSetQuery<Category>(paging: false).ToList();
            var units = taxi.GetDataSetQuery<Unit>(paging: false).ToList();

            // 2. 定义配件名称的随机池，让数据看起来更像汽配
            var partNames = new[] { "滤清器", "制动片", "火花塞", "减震器", "雨刮片", "蓄电池", "正时皮带", "控制臂", "点火线圈", "发电机" };
            var brands = new[] { "博世(Bosch)", "德尔福(Delphi)", "马勒(Mahle)", "采埃孚(ZF)", "电装(Denso)" };

            // 3. 配置 Bogus 生成规则
            var partFaker = new Faker<Part>("zh_CN") // 使用中文数据
                .RuleFor(p => p.Id, f => Guid.NewGuid())
                .RuleFor(p => p.PartName, f => f.PickRandom(partNames) + " " + f.Commerce.ProductAdjective())
                .RuleFor(p => p.OECode, f => f.Random.Replace("OE-##-?????-####").ToUpper()) // 生成像OE码的字符串
                .RuleFor(p => p.Brand, f => f.PickRandom(brands))
                .RuleFor(p => p.Model, f => f.Vehicle.Model())
                .RuleFor(p => p.CategoryId, f => f.PickRandom(categories).Id)
                .RuleFor(p => p.UnitId, f => f.PickRandom(units).Id)
                .RuleFor(p => p.CostPrice, f => f.Finance.Amount(50, 500))
                .RuleFor(p => p.SellingPrice, (f, p) => p.CostPrice * 1.5m) // 售价是进价的1.5倍
                .RuleFor(p => p.MinStock, f => f.Random.Number(5, 20))
                .RuleFor(p => p.MaxStock, f => f.Random.Number(100, 500))
                .RuleFor(p => p.CreatedAt, f => f.Date.Past(1));

            var parts = partFaker.Generate(1);
            return UsualResult(parts.FirstOrDefault());
        }
    }
}
