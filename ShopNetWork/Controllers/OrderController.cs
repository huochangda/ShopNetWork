using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Model.Shop;
using RabbitMQ.Client;
using ShopNet.Core;
using ShopNetWork.Interfaces;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using Yitter.IdGenerator;

namespace ShopNetWork.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IGoodTypeServices goodTypeServices;//用户服务
        private readonly ISqlSugarClient db;//sqlsugar上下文
        private readonly RedisCache cacheService;//缓存
        private readonly ILogger<UserController> logger;//日志
        private readonly IModel model;//rabbit队列创建
        private readonly IConnection con;//rabbit交换机创建
        private readonly IUnitOfWork unitOfWork;//sqlsugar事务

        public OrderController(ISqlSugarClient db, IGoodTypeServices goodTypeServices, RedisCache cacheService,
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

        [HttpGet("CreateSonwFlakeId")]
        public IActionResult CreateSonwFlakeId(int num = 10)
        {
            try
            {
                //存放雪花id
                List<Int64> list = new List<long>();
                for (int i = 0; i < num; i++)
                {
                    list.Add(YitIdHelper.NextId());
                }
                return Ok(list);
            }
            catch (System.Exception ex)
            {
                logger.LogError($"雪花id出错{ex.Message}");
                throw;
            }
        }

        [HttpGet("OrderList")]
        public IActionResult OrderList(string ordercode, DateTime? begintime = null, DateTime? endtime = null, string name = null, int state = 0, int paytype = 0)
        {
            var list = db.Queryable<Order>();
            if (!string.IsNullOrEmpty(name))
            {
                list = list.Where(a => a.UserName.Contains(name));
            }
            if (!string.IsNullOrEmpty(ordercode))
            {
                list = list.Where(a => a.OrderCode.Contains(ordercode));
            }
            if (begintime != null)
            {
                list = list.Where(a => a.AddTime >= (begintime));
            }
            if (endtime != null)
            {
                list = list.Where(a => a.AddTime <= (endtime));
            }
            if (paytype != 0)
            {
                list = list.Where(a => ((int)a.PayType) == paytype);
            }
            if (state != 0)
            {
                list = list.Where(a => ((int)a.OrderState) == (state));
            }
            return Ok(list);
        }

        [HttpGet("GetEnumOrderState")]
        public IActionResult GetEnumOrderState()
        {
            OrderState state = new OrderState();
            Dictionary<string, int> list = Enum.GetValues(typeof(OrderState))
            .Cast<OrderState>()
            .ToDictionary(enumVal => enumVal.ToString(), enumVal => (int)enumVal);
            var list2 = list.Select(a => new
            {
                label = a.Key,
                value = a.Value.ToString()
            });
            return Ok(list2);
        }

        [HttpGet("GetEnumOrderType")]
        public IActionResult GetEnumOrderType()
        {
            OrderState state = new OrderState();
            Dictionary<string, int> list = Enum.GetValues(typeof(PayType))
            .Cast<PayType>()
            .ToDictionary(enumVal => enumVal.ToString(), enumVal => (int)enumVal);
            var list2 = list.Select(a => new
            {
                label = a.Key,
                value = a.Value.ToString()
            });
            return Ok(list2);
        }
    }
}