using MathNet.Numerics.LinearAlgebra.Factorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Model;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Crmf;
using RabbitMQ.Client;
using RestSharp.Portable;
using RestSharp.Portable.HttpClient;
using ShopNet.Core;
using ShopNetWork.Interfaces;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ShopNetWork.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExpressDeliveryController : ControllerBase
    {
        private readonly IGoodTypeServices goodTypeServices;//用户服务
        private readonly ISqlSugarClient db;//sqlsugar上下文
        private readonly RedisCache cacheService;//缓存
        private readonly ILogger<UserController> logger;//日志
        private readonly IModel model;//rabbit队列创建
        private readonly IConnection con;//rabbit交换机创建
        private readonly IUnitOfWork unitOfWork;//sqlsugar事务

        public ExpressDeliveryController(ISqlSugarClient db, IGoodTypeServices goodTypeServices, RedisCache cacheService,
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

        /// <summary>
        /// 申通快递查询
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        [HttpGet("STExpressDelivery")]
        public async Task<IActionResult> STExpressDelivery(Int64 pid = 777137915737923)
        {
            var requestData = new { ShipperCode = "STO", LogisticCode = $"{pid}" };
            var json = JsonConvert.SerializeObject(requestData);

            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://api.kdniao.com/Ebusiness/EbusinessOrderHandle.aspx"),
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "RequestData", json },
            { "DataType", "2" },
            { "EBusinessID", "1735391" },
            { "DataSign", "NzBhYWZkNjAzNTE2ZjQzNTI4ZDQ3NGIyNjUwYzJhZDU=" },
            { "RequestType", "1002" },
        }),
            };
            request.Headers.Add("Accept", "application/json");
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            using (var response = await client.SendAsync(request))
            {
                // 处理响应结果
                var resultString = await response.Content.ReadAsStringAsync();
                return Ok(resultString);
            }
        }
    }
}