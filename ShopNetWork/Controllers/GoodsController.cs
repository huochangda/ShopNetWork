using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Model;
using Model.Shop;
using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;
using RabbitMQ.Client;
using ShopNet.Core;
using ShopNetWork.Interfaces;
using SqlSugar;
using System.Collections.Generic;
using System.Linq;

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

        [HttpGet("GetGoodTypeStatic")]
        public IActionResult GetGoodTypeStatic()
        {
            try
            {
                var list = db.Queryable<Goods>().ToList().GroupBy(a => a.GoodTypeId);
                var list2 = list.Select(g => new
                {
                    typeid = g.Key,
                    count = g.Sum(a => a.Num),
                });
                var list3 = list2.Join(db.Queryable<GoodType>().ToList(), a => a.typeid, b => b.GoodTypeId, (a, b) => new
                {
                    typename = b.TypeName,
                    Count = a.count,
                }).ToList();
                return Ok(new { list3, list });
            }
            catch (System.Exception ex)
            {
                logger.LogError($",报错信息{ex.Message}");
                throw;
            }
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

        /// <summary>
        /// 获取商品列表
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">页大小</param>
        /// <param name="gName">商品名称</param>
        /// <param name="bId">品牌编号</param>
        /// <param name="gtId">商品类别编号</param>
        /// <param name="sDate">开始时间</param>
        /// <param name="eDate">结束时间</param>
        /// <returns></returns>
        [HttpGet("GetGoodsList")]
        public IActionResult GetGoodsList(int pageIndex, int pageSize, string? gName, int bId, int gtId, string? sDate, string? eDate)
        {
            int totalCount = 0;
            int pageCount = 0;
            var data = goodTypeServices.GetGoodsList(pageIndex, pageSize, out totalCount, out pageCount
               , gName, bId, gtId, sDate, eDate);
            return Ok(new
            {
                data = data,
                totalCount = totalCount,
                pageCount = pageCount,
            });
        }

        /// <summary>
        /// 获取商品属性类别
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// 获取品牌列表
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// 获取商品属性
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("GetGoodsProp")]
        public IActionResult GetGoodsProp(int id)
        {
            try
            {
                var list = db.Queryable<GoodsProp>().Where(a => a.GPTId == id).ToList();
                return Ok(list);
            }
            catch (System.Exception ex)
            {
                logger.LogError($"删除商品分类,报错信息{ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 商品增加
        /// </summary>
        /// <param name="goods"></param>
        /// <returns></returns>
        [HttpPost("GoodsAdd")]
        public IActionResult GoodsAdd(Goods goods)
        {
            try
            {
                ///RabbitMq案例
                RabbitHelper.rabbitmq.Receive(GoodsAddDataMq.exchangeName,
                    GoodsAddDataMq.routingKey,
                    GoodsAddDataMq.queueName,
                    (msg) =>
                    {
                        Goods goods = JsonConvert.DeserializeObject<Goods>(msg);
                        var list = db.Insertable(goods).RemoveDataCache().ExecuteCommand();
                    }, GoodsAddDataMq.exchangeType
                    );

                RabbitHelper.rabbitmq.Send(GoodsAddDataMq.exchangeName, GoodsAddDataMq.routingKey, GoodsAddDataMq.queueName, goods, GoodsAddDataMq.exchangeType);

                //var list = db.Insertable(goods).RemoveDataCache().ExecuteCommand();

                return Ok(1);
            }
            catch (System.Exception ex)
            {
                logger.LogError($"删除商品分类,报错信息{ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 商品修改
        /// </summary>
        /// <param name="goods"></param>
        /// <returns></returns>
        [HttpPost("GoodsUpdate")]
        public IActionResult GoodsUpdate(Goods goods)
        {
            try
            {
                var list = db.Updateable(goods).RemoveDataCache().ExecuteCommand();
                return Ok(list);
            }
            catch (System.Exception ex)
            {
                logger.LogError($"修改商品错误,报错信息{ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 商品删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("GoodsDelete")]
        public IActionResult GoodsDelete(int id)
        {
            try
            {
                Goods goods = db.Queryable<Goods>().Where(a => a.GoodId == id).ToList()[0];
                goods.IsOpen = false;
                var list = db.Updateable(goods).RemoveDataCache().ExecuteCommand();
                return Ok(list);
            }
            catch (System.Exception ex)
            {
                logger.LogError($"删除商品错误,报错信息{ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 商品反填
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("GoodsBackFillById")]
        public IActionResult GoodsBackFillById(int id)
        {
            try
            {
                var list = db.Queryable<Goods>().Includes(a => a.GoodsPropType).Includes(a => a.Brand).Where(a => a.IsOpen == true && a.GoodId == id).ToList(); ;
                return Ok(list);
            }
            catch (System.Exception ex)
            {
                logger.LogError($"反填商品错误,报错信息{ex.Message}");
                throw;
            }
        }
    }
}