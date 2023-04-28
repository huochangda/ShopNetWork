using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Model;
using Model.Shop;
using RabbitMQ.Client;
using ShopNet.Core;
using ShopNetWork.Interfaces;
using SqlSugar;
using System.Collections.Generic;

namespace ShopNetWork.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GoodsController : ControllerBase
    {
        private readonly IGoodTypeServices goodTypeServices;//用户服务
        private readonly ISqlSugarClient db;//sqlsugar上下文
        private readonly RedisCache cacheService;//缓存
        private readonly ILogger<UserController> logger;//日志
        private readonly IModel model;//rabbit队列创建
        private readonly IConnection con;//rabbit交换机创建
        private readonly IUnitOfWork unitOfWork;//sqlsugar事务

        public GoodsController(ISqlSugarClient db, IGoodTypeServices goodTypeServices, RedisCache cacheService,
            ILogger<UserController> logger, IConnection con, IModel model, IUnitOfWork unitOfWork)
        {
            this.con = con;
            this.model = model;
            this.logger = logger;
            this.cacheService = cacheService;
            this.db = db;
            this.goodTypeServices = goodTypeServices;
            this.unitOfWork = unitOfWork;
        }

        [HttpGet("GetTree")]
        public List<Tree> GetTree(int pid)
        {
            try
            {
                List<Tree> tree = goodTypeServices.GetTree(pid);
                return tree;
            }
            catch (System.Exception ex)
            {
                logger.LogError($",报错信息{ex.Message}");
                throw;
            }
        }

        [HttpPost("UpdateGoodType")]
        public IActionResult UpdateGoodType(GoodType goodType)
        {
            try
            {
                GoodType goodstype = goodTypeServices.GetId(goodType.GoodTypeId);
                goodstype.TypeName = goodType.TypeName;
                var list = goodTypeServices.Update(goodstype);
                return Ok(list);
            }
            catch (System.Exception ex)
            {
                logger.LogError($",报错信息{ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 删除商品分类
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("DeleteGoodType")]
        public IActionResult DeleteGoodType(int id)
        {
            try
            {
                var list = goodTypeServices.Delete(id);
                return Ok(list);
            }
            catch (System.Exception ex)
            {
                logger.LogError($"删除商品分类,报错信息{ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 添加商品分类
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("AddGoodType")]
        public IActionResult AddGoodType(GoodType goodType)
        {
            try
            {
                var list = goodTypeServices.Add(goodType);
                return Ok(list);
            }
            catch (System.Exception ex)
            {
                logger.LogError($"添加商品分类,报错信息{ex.Message}");
                throw;
            }
        }

        [HttpGet("GetGoodsList")]
        public IActionResult GetGoodsList(int pageIndex, int pageSize, string? gName, int bId, int gtId, string? sDate, string? eDate)
        {
            int totalCount = 0;
            int pageCount = 0;
            var data= goodTypeServices.GetGoodsList(pageIndex, pageSize, out totalCount, out pageCount
               , gName, bId, gtId, sDate, eDate);
            return Ok(new
            {
                data=data,
                totalCount=totalCount,
                pageCount=pageCount,
            });
        }

        [HttpGet("GetGoodsPropType")]
        public IActionResult GetGoodsPropType()
        {
            try
            {
                var list = db.Queryable<GoodsPropType>().ToList();
                return Ok(list);
            }
            catch (System.Exception ex)
            {
                logger.LogError($"删除商品分类,报错信息{ex.Message}");
                throw;
            }
        }

        [HttpGet("GetBoand")]
        public IActionResult GetBoand()
        {
            try
            {
                var list = db.Queryable<Brand>().ToList();
                return Ok(list);
            }
            catch (System.Exception ex)
            {
                logger.LogError($"删除商品分类,报错信息{ex.Message}");
                throw;
            }
        }
        [HttpGet("GetGoodsProp")]
        public IActionResult GetGoodsProp(int id)
        {
            try
            {
                var list = db.Queryable<GoodsProp>().Where(a=>a.GPTId==id).ToList();
                return Ok(list);
            }
            catch (System.Exception ex)
            {
                logger.LogError($"删除商品分类,报错信息{ex.Message}");
                throw;
            }
        }
        [HttpPost("GoodsAdd")]
        public IActionResult GoodsAdd(Goods goods)
        {
            try
            {

                var list = db.Insertable(goods).RemoveDataCache().ExecuteCommand();
                return Ok(list);
            }
            catch (System.Exception ex)
            {
                logger.LogError($"删除商品分类,报错信息{ex.Message}");
                throw;
            }
        }

    }
}