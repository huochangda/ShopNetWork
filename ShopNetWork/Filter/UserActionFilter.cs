using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Model.Dto;
using Newtonsoft.Json;
using ShopNetWork.Extensions;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;

namespace ShopNetWork.Filter
{
    public class UserActionFilter : ActionFilterAttribute,IActionFilter
    {
        private readonly ILogger<UserActionFilter> _logger;
        private readonly IHttpContextAccessor httpContextAccessor;
        public UserActionFilter(ILogger<UserActionFilter> logger, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            this.httpContextAccessor = httpContextAccessor;
        }

     
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            //获取方法头部
            var authHeader = httpContextAccessor.HttpContext.Request.Headers["Authorization"];
            //创建jwt类型
            var tokenHandler = new JwtSecurityTokenHandler();
            //创建秘钥
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(TokenConfig.secret)),//秘钥设置
                ValidIssuer = TokenConfig.issuer,
                ValidAudience = TokenConfig.audience,
                ValidateIssuer = false,
                ValidateAudience = false
            };
            //jwt token解码编译
            var claimsPrincipal = tokenHandler.ValidateToken(authHeader.ToString().Substring(7), validationParameters, out _);
            // 获取 JWT Token 中的名称为 sub 的声明
            //获取头部用户数据
            var username = claimsPrincipal.Claims.AsEnumerable().ToList()[0].ToString().Split(':')[2];
            var password = claimsPrincipal.Claims.AsEnumerable().ToList()[1].ToString().Split(':')[2];
            //参数获取
            string id = JsonConvert.SerializeObject(context.ActionArguments.Values);
            //日志写入
          _logger.LogError($"UserActionFilter用户{username}密码{password}使用了方法Executing action" +
           $" {context.ActionDescriptor.DisplayName},params:参数为:{id}");
        }
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            var authHeader = httpContextAccessor.HttpContext.Request.Headers["Authorization"];
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(TokenConfig.secret)),//秘钥设置
                ValidIssuer = TokenConfig.issuer,
                ValidAudience = TokenConfig.audience,
                ValidateIssuer = false,
                ValidateAudience = false
            };
            var claimsPrincipal = tokenHandler.ValidateToken(authHeader.ToString().Substring(7), validationParameters, out _);
            // 获取 JWT Token 中的名称为 sub 的声明
            var username = claimsPrincipal.Claims.AsEnumerable().ToList()[0].ToString().Split(':')[2];
            var password = claimsPrincipal.Claims.AsEnumerable().ToList()[1].ToString().Split(':')[2];

            _logger.LogInformation($"UserActionFilter用户{username}密码{password}使用了方法Executing action" +
                $" {context.ActionDescriptor.DisplayName}");
        }
    }
}
