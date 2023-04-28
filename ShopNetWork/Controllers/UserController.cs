using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Model;
using Model.Dto;
using Model.Shop;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ShopNet.Core;
using ShopNetWork.Interfaces;
using SqlSugar;
using System;
using System.Linq;
using System.Text;

namespace ShopNetWork.Controllers
{
    /// <summary>
    /// 用户控制器
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly RedisCache cacheService;
        private readonly IConnection con;
        private readonly ISqlSugarClient db;
        //sqlsugar上下文
        //缓存
        private readonly ILogger<UserController> logger;

        //日志
        private readonly IModel model;

        private readonly IUserServices userServices;//用户服务
                                                    //rabbit队列创建
                                                    //rabbit交换机创建


        /// <summary>
        /// 用户服务接口
        /// </summary>
        public UserController(ISqlSugarClient db, IUserServices userServices, RedisCache cacheService,
            ILogger<UserController> logger, IConnection con, IModel model)
        {
            this.con = con;
            this.model = model;
            this.logger = logger;
            this.db = db;
            this.cacheService = cacheService;
            this.userServices = userServices;
        }

        /// <summary>
        /// redis
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("redis")]
        public object redis(int id = 0)
        {
            cacheService.Add("string", "fewfwe");
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
        /// 批量修改状态
        /// </summary>
        /// <returns></returns>
        [HttpGet("AllUpdateState")]
        public IActionResult AllUpdateState(string ids, bool isopen)
        {
            try
            {
                var respose = userServices.AllUpdateState(ids, isopen);
                return Ok(respose);
            }
            catch (Exception ex)
            {
                logger.LogError($"报错信息{ex.Message}");
                throw;
            }
        }

        [HttpGet("DeleteUser")]
        public IActionResult DeleteUser(int id = 0)
        {

            try
            {
                var respose = userServices.DeleteUser(id);
                return Ok(respose);
            }
            catch (Exception ex)
            {
                logger.LogError($"报错信息{ex.Message}");
                throw;
            }

        }

        /// <summary>
        /// 获取用户地址下拉列表
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetAddressList")]
        public IActionResult GetAddressList()
        {
            try
            {
                var data = userServices.GetAddressList();
                return Ok(data);
            }
            catch (Exception ex)
            {
                logger.LogError($"报错信息{ex.Message}");
                throw;
            }

        }

        /// <summary>
        /// 获取用户年龄姓名统计
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetAgeStatistics")]
        public IActionResult GetAgeStatistics()
        {

            try
            {
                var data1 = userServices.GetAgeStatistics();
                return Ok(data1);
            }
            catch (Exception ex)
            {
                logger.LogError($"报错信息{ex.Message}");
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


            try
            {
                var countList = userServices.GetLoginLog();
                return Ok(countList);
            }
            catch (Exception ex)
            {
                logger.LogError($"报错信息{ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 获取角色下拉列表
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetRoleList")]
        public IActionResult GetRoleList()
        {

            try
            {
                var data = userServices.GetRoleList();
                return Ok(data);
            }
            catch (Exception ex)
            {
                logger.LogError($"报错信息{ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 批量假删除
        /// </summary>
        /// <returns></returns>
        [HttpGet("AllDelete")]
        public IActionResult GetRoleList(string ids)
        {


            try
            {
                var respose = userServices.GetRoleList(ids);
                return Ok(respose);
            }
            catch (Exception ex)
            {
                logger.LogError($"报错信息{ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 删除用户假删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <summary>
        /// 反填用户信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("GetUserById")]
        public IActionResult GetUserById(int id)
        {

            try
            {
                var respose = userServices.GetFirst(a => a.UserId == id);
                return Ok(respose);
            }
            catch (Exception ex)
            {
                logger.LogError($"报错信息{ex.Message}");
                throw;
            }

        }

        

        
        /// <summary>
        /// 修改用户信息
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost("UpdateUser")]
        public IActionResult UpdateUser(User user)
        {
            try
            {
                var respose = userServices.Update(user);
                return Ok(respose);
            }
            catch (Exception ex)
            {
                logger.LogError($"报错信息{ex.Message}");
                throw;
            }
            
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
                var response = userServices.UserList(parm);

                return Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError($"列表显示错误,错误信息为{ex.Message}");
                throw;
            }
        }
    }
}