﻿using Microsoft.AspNetCore.Http;
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

        /// <summary>
        /// 订单列表
        /// </summary>
        /// <param name="ordercode"></param>
        /// <param name="begintime"></param>
        /// <param name="endtime"></param>
        /// <param name="name"></param>
        /// <param name="state"></param>
        /// <param name="paytype"></param>
        /// <returns></returns>
        [HttpGet("OrderList")]
        public IActionResult OrderList(int pageIndex, int pageSize, string ordercode, string?
            begintime = null, string? endtime = null, string name = null, int state = 0, int paytype = 0)
        {
            #region

            int pageCount = 0;
            int totalCount = 0;

            var list = (from o in db.Queryable<Order>().ToList()
                        join og in db.Queryable<OrderGoods>().ToList()
                        on o.OrderId equals og.OrderId
                        join g in db.Queryable<Goods>().ToList()
                        on og.GoodId equals g.GoodId
                        where (string.IsNullOrEmpty(ordercode) || o.OrderCode.Contains(ordercode))
                        && (string.IsNullOrEmpty(begintime) || o.AddTime >= DateTime.Parse(begintime))
                        && (string.IsNullOrEmpty(endtime) || o.AddTime < DateTime.Parse(endtime).AddDays(1))
                        && (string.IsNullOrEmpty(name) || o.UserName.Contains(name))
                        && (paytype == 0 || ((int)o.PayType) == paytype)
                        && (state == 0 || ((int)o.OrderState) == state)
                        select o);

            foreach (var item in list)
            {
                item.goods = (from og in db.Queryable<OrderGoods>().ToList()
                              join g in db.Queryable<Goods>().ToList()
                           on og.GoodId equals g.GoodId
                              where (og.OrderId == item.OrderId)
                              select g).ToList();
            }

            totalCount = list.Count();
            pageCount = (int)Math.Ceiling(totalCount * 1.0 / pageSize);
            list = list.Skip((pageIndex - 1) * pageSize).Take(pageSize);

            return Ok(new
            {
                totalCount = totalCount,
                pageCount = pageCount,
                list = list.ToList()
            });

            #endregion 莫总代码
        }

        /// <summary>
        /// 获取枚举订单状态
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// 获取枚举支付类型
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetEnumPayType")]
        public IActionResult GetEnumPayType()
        {
            var a = typeof(Order);
            var b = Enum.GetValues<PayType>();
            List<object> list = new List<object>();
            foreach (var item in Enum.GetValues<PayType>())
            {
                list.Add(new
                {
                    name = item.ToString(),
                    value = item,
                });
            }
            return Ok(list);
        }
    }
}