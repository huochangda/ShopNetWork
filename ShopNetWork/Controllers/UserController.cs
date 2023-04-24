using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Model;
using ShopNetWork.Extensions;
using SqlSugar;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System;
using ShopNetWork.Interfaces;
using ShopNet.Common.Helpers;
using Ex.Common;
using Model.Dto;
using ShopNet.Core;
using System.Linq;
using Model.Shop;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;
using System.Threading.Channels;
using RabbitMQ.Client;
using NPOI.SS.UserModel;
using SqlSugar.Extensions;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using ShopNetWork.Filter;
using NPOI.SS.Formula.Functions;
using System.Collections.Generic;

namespace ShopNetWork.Controllers
{
    /// <summary>
    /// 用户控制器
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    //[ServiceFilter(typeof(UserActionFilter))]
    public class UserController : ControllerBase
    {
        private readonly IUserServices userServices;
        private readonly ISqlSugarClient db;
        private readonly RedisCache cacheService;
        private readonly ILogger<UserController> logger;
        private readonly IModel model;
        private readonly IConnection con;
         private readonly IUnitOfWork unitOfWork;
        /// <summary>
        /// 用户服务接口
        /// </summary>
        public UserController(ISqlSugarClient db, IUserServices userServices, RedisCache cacheService,
            ILogger<UserController> logger, IConnection con,IModel model, IUnitOfWork unitOfWork)
        {
            this.con = con;
            this.model = model;
            this.logger = logger;
            this.cacheService = cacheService;
            this.db = db;
            this.userServices = userServices;
            this.unitOfWork = unitOfWork;
        }


        /// <summary>
        /// redis
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("redis")]
        public object redis(int id = 0)
        {
            cacheService.Add("string","fewfwe");
            return 1;

        }
        /// <summary>
        /// rabbit
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("rabbit")]
        public object rabbit(int id = 0)
        {
           
            var message = "";
                model.QueueDeclare(queue: "hello", durable: false, exclusive: false, autoDelete: false, arguments: null);
                var consumer = new EventingBasicConsumer(model);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                     message = Encoding.UTF8.GetString(body);
                    Console.WriteLine("Received message: {0}", message);
                };
            model.BasicConsume(queue: "hello", autoAck: true, consumer: consumer);
            return message;
            
        }



        /// <summary>
        /// 删除用户假删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        
        [HttpGet("DeleteUser")]
        public IActionResult DeleteUser(int id = 0)
        {
            unitOfWork.BeginTran();
            if (id <= 0)
            {
                return Ok(-1);
            }
            var a = userServices.GetFirst(a => a.UserId == id);
            a.IsOpen = false;
            var respose = userServices.Update(a);
            unitOfWork.CommitTran();
            return Ok(respose);

        }
        /// <summary>
        /// 反填用户信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("GetUserById")]
        public IActionResult GetUserById(int id)
        {
            var respose = userServices.GetFirst(a => a.UserId == id);
            return Ok(respose);
        }
        /// <summary>
        /// 修改用户信息
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost("UpdateUser")]
        public IActionResult UpdateUser(User user)
        {
            var respose = userServices.Update(user);
            return Ok(respose);
        }

        
        

        /// <summary>
        /// 获取用户列表
        /// </summary>
        /// <returns></returns>
        [HttpPost("UserList")]
        public IActionResult UserList(UserQueryDto parm)
        {
            try
            {

                var predicate = Expressionable.Create<User>();
                predicate = predicate.And(m => m.IsOpen == true);
                predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.username), m => m.UserName.Contains(parm.username));
                predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.truename), m => m.TrueName.Contains(parm.truename));
                predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.address), m => m.UserAddress.Contains(parm.address));
                
                //cacheService.GetOrCreate<Expressionable<User>>("string0", 
                //    ( ) => { return predicate.AndIF(!string.IsNullOrEmpty(parm.address), m => m.UserAddress.Contains(parm.address));  },100);

                var response = userServices.GetPages(predicate.ToExpression(), parm);
                ISugarQueryable<User> queryable =db.Queryable<User>();
                
                // 遍历List<T>并添加查询条件
                response.DataSource.ForEach(item => {
                    queryable = queryable.Where(t => t.IsOpen == item.IsOpen);
                });
               response.DataSource= queryable.Includes(a=>a.Role).ToList();
                //  cacheService.Add("string1", JsonConvert.SerializeObject(response));
                //var pr=JsonConvert.DeserializeObject<PagedInfo<User>>(cacheService.Get<string>("string1"));
                //bool tr = pr.Equals(response);
                return Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError($"列表显示错误,错误信息为{ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 获取用户登录次数统计
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetLoginLog")]
        public IActionResult GetLoginLog()
        {
            //var predicate = db.Queryable<LoginLog>().ToList().GroupBy(l => new { l.LoginTime.Year, l.LoginTime.Month });
            //var countList = predicate.Select(l => new
            //{
            //    yearMonth=$"{l.Key.Year}-{l.Key.Month}-{l.Key.Month.ToString("00")}",
            //    total=l.Count()
            //});
            var countList = db.Queryable<LoginLog>()
                  .ToList()
                  .GroupBy(l => new { l.LoginTime.Year, l.LoginTime.Month })
                  .Select(g => new
                  {
                      YearMonth = $"{g.Key.Year}-{g.Key.Month.ToString("00")}",
                      Count = g.Count()
                  })
                  .ToList();
            return Ok(countList);

        }

        /// <summary>
        /// 获取用户年龄姓名统计
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetAgeStatistics")]
        public IActionResult GetAgeStatistics()
        {
            //var data = db.Queryable<User>().ToList().Where(an => an.IsOpen == true);
            var data1 = db.Queryable<User>().Where(a=>a.IsOpen==true).Includes(a=>a.Role).ToList();
            return Ok(data1);

        }
        /// <summary>
        /// 获取用户地址下拉列表
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetAddressList")]
        public IActionResult GetAddressList()
        {
            var data = db.Queryable<User>().ToList().GroupBy(a => new { a.UserAddress });
            return Ok(data);
        }

        /// <summary>
        /// 获取用户地址下拉列表
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetRoleList")]
        public IActionResult GetRoleList()
        {
            var data = db.Queryable<Role>().ToList();
            return Ok(data);
        }


    }
}
