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
        public IActionResult OrderList(int orderid, string ordercode, int paytype, int state, DateTime begintime, DateTime endtime)
        {
            var list = db.Queryable<Order>().ToList();
            return Ok(list);
        }
    }
}