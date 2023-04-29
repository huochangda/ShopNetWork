using Ex.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Model;
using RabbitMQ.Client;
using ShopNet.Core;
using ShopNetWork.Extensions;
using ShopNetWork.Interfaces;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace ShopNetWork.Controllers
{
    /// <summary>
    /// 权限控制器
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserServices userServices;
        private readonly ISqlSugarClient db;
        private readonly RedisCache cacheService;
        private readonly ILogger<UserController> logger;
        private readonly IUnitOfWork unitOfWork;

        /// <summary>
        /// 用户服务接口
        /// </summary>
        public AuthController(ISqlSugarClient db, IUserServices userServices, RedisCache cacheService,
            ILogger<UserController> logger, IConnection con, IModel model, IUnitOfWork unitOfWork)
        {
            this.logger = logger;
            this.cacheService = cacheService;
            this.db = db;
            this.userServices = userServices;
            this.unitOfWork = unitOfWork;
        }

        [HttpGet("GetToken")]
        public string GetToken()
        {
            //载荷增加用户
            var claims = new[] {
             new Claim(ClaimTypes.Name,"yinmingneng"),
             new Claim(ClaimTypes.Email,"123@qq.com")
            };
            //加密密钥
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(TokenConfig.secret));
            ///签名
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var jwttoken = new JwtSecurityToken(TokenConfig.issuer, TokenConfig.audience, claims, DateTime.Now
                , DateTime.Now.AddMinutes(TokenConfig.accessExpiration), credentials);
            var token = new JwtSecurityTokenHandler().WriteToken(jwttoken);
            return token;
        }

        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="autocode">验证码</param>
        /// <param name="uuid">GUID</param>
        /// <returns></returns>
        [HttpPost("UserLogin")]
        public IActionResult UserLogin(string username, string password, string autocode, string uuid)
        {
           // List<User> response = cacheService.Get<List<User>>("User");
            
           //var res= response.AsEnumerable().Where(a => a.UserName == username && a.PassWord == password);
            
            //if(res.Count()==0)
            //{
              var  response = db.Queryable<User>().Where(a => a.UserName == username && a.PassWord == password).ToList();
            //    if (response.Count > 0)
            //    {
            //        cacheService.Add<List<User>>("User",response,30);
            //    }else
            //    {
            //        return Ok(-1);
            //    }
            //}

            var code = HttpContext.Session.GetString(uuid.ToString());

            if (autocode.ToLower() == code.ToLower() && response.Count > 0)
            {
                var claims = new[] {
             new Claim(ClaimTypes.Name,username),
             new Claim(ClaimTypes.Upn,password)
            };
                //加密密钥
                var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(TokenConfig.secret));
                ///签名
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var jwttoken = new JwtSecurityToken(TokenConfig.issuer, TokenConfig.audience, claims, DateTime.Now
                    , DateTime.Now.AddMinutes(TokenConfig.accessExpiration), credentials);
                var token = new JwtSecurityTokenHandler().WriteToken(jwttoken);
                return Ok(new { state = 1, userinfo = response, token = token });
            }
            else
            {
                return Ok(-1);
            }
        }

        /// <summary>
        /// 用户注册
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost("UserRegister")]
        public IActionResult UserRegister(User user)
        {
            if (string.IsNullOrEmpty(user.UserName) || string.IsNullOrEmpty(user.PassWord))
            {
                return Ok(-1);
            }
            var response = userServices.Add(user);
            return Ok(response);
        }

        /// <summary>
        /// 返回验证码
        /// </summary>
        /// <returns></returns>
        [HttpGet("CheckCode")]
        public ActionResult CheckCode(string uuid)
        {
            var code = ValidateCodeHelper.CreateRandomCode(4);
            //把生成的随机数,存到session
            HttpContext.Session.SetString(uuid.ToString(), code);
            var a = HttpContext.Session.GetString(uuid.ToString());
            var b = ValidateCodeHelper.CreateValidateGraphic(code);
            string str = Convert.ToBase64String(b);
            return Ok("data:image/jpeg;base64," + str);
        }

        /// <summary>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("DeleteRole")]
        public IActionResult DeleteRole(int id)
        {
            var users = db.Deleteable<User>().Where(a => a.UserId == id).ExecuteCommand();
            return Ok(users);
        }

        [HttpPost("AddRole")]
        public IActionResult AddRole(User user)
        {
            var response = db.Insertable(user).RemoveDataCache().ExecuteCommand();
            return Ok(response);
        }

        [HttpPost("UpdateRole")]
        public IActionResult UpdateRole(User user)
        {
            var users = db.Updateable(user).ExecuteCommand();
            return Ok(users);
        }
    }
}