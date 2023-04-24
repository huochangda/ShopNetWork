using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NLog.Extensions.Logging;
using NLog.Web;
using Org.BouncyCastle.Asn1.X509.Qualified;
using RabbitMQ.Client;
using ShopNet.Core;
using ShopNetWork.Controllers;
using ShopNetWork.Extensions;
using ShopNetWork.Filter;
using ShopNetWork.Middleware;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static ShopNet.Core.ReidsServer;

namespace ShopNetWork
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();

            RedisServer.Initalize();//开启redis缓存服务

            services.AddSingleton<IConnection>(sp =>
            {
                var factory = new ConnectionFactory()
                {
                    HostName = "localhost", // RabbitMQ 服务器主机名
                    UserName = "guest",     // RabbitMQ 用户名
                    Password = "guest"      // RabbitMQ 密码
                };
                return factory.CreateConnection();
            });//rabbit交换价创建注入

            services.AddScoped<IModel>(sp =>
            {
                var connection = sp.GetRequiredService<IConnection>();
                return connection.CreateModel();
            });//rabbit队列创建

            
            services.AddAutoIOC();//自动依赖注入
            //services.AddScoped ( typeof(IBaseService<>),typeof(BaseService<>)) ;泛型依赖注入
            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(10); // 设置Session过期时间为10分钟
            });

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => false;//关闭欧盟协议
                options.MinimumSameSitePolicy = SameSiteMode.None;//使用默认方案
  
            });

            //services.AddSingleton<MyMiddleware>();自定义中间件注入

            services.AddScoped<UserActionFilter>();//用户方法跟踪过滤器注入

            //services.AddControllers(options =>
            //{
            //    options.Filters.Add(typeof(UserActionFilter));
            //});全局过滤器

            services.AddScoped<RedisCache>();

            #region swagger 配置
            //swagger添加报文头，方便做登录和退出：
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "_3315work", Version = "v1" });
                //配置注释显示swagge ui当中
                var xmlfile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlpath = Path.Combine(AppContext.BaseDirectory, xmlfile);
                c.IncludeXmlComments(xmlpath,true);
                //Token绑定到configureServices
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,//jwt默认存放Authorization信息的位置(请求头中)
                    Type = SecuritySchemeType.ApiKey,
                    Description = "直接在下框中输入Bearer {token}（注意两者之间是一个空格）",
                    Name = "Authorization",//jwt默认的参数名称
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
          {
            new OpenApiSecurityScheme
            {
              Reference=new OpenApiReference
              {
                Type=ReferenceType.SecurityScheme,
                Id="Bearer"
              }
            },
            new string[] {}
          }
        });
            });
            #endregion

            #region SqlSugar
            // 添加 SqlSugar
            services.AddSqlsugarSetup();

            #endregion

            #region 跨域
            services.AddCors(options =>
            {
                options.AddPolicy("ShopNet"//请求名称
                    , policy =>
                    {
                        policy.SetIsOriginAllowed((host) => true)//基础跨域策略
                        .AllowAnyMethod()                       //容许所有请求方法
                        .AllowAnyHeader()                       //容许任何的头部来源
                        .AllowCredentials()                     //容许任何的请求证书
                        .WithExposedHeaders("Content-Disposition");//
                    });
            });
            #endregion

            #region 日志配置
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                loggingBuilder.AddNLog();
            });
            #endregion

            #region jwt配置
            services.AddAuthentication(options => {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x => {

                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(TokenConfig.secret)),//秘钥设置
                    ValidIssuer = TokenConfig.issuer,
                    ValidAudience = TokenConfig.audience,
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
                x.Events = new JwtBearerEvents()
                {
                    OnMessageReceived = context =>
                    {
                        context.Token = context.Request.Query["access_token"];
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        // 如果过期，则把<是否过期>添加到，返回头信息中
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Add("Token-Expired", "true");
                        }
                        return Task.CompletedTask;
                    }
                };

            });
            #endregion

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            #region jwt
            //在Configure添加授权和鉴权的组件：
            app.UseAuthentication();//开启认证
            app.UseAuthorization();//开启授权
            #endregion

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ShopNetWork v1"));
                
            }

            app.UseCookiePolicy();//使用cookie配置

            app.UseSession();//使用session

            //app.UseRabbitMqReceived();使用rabbitmq中间件

            app.UseCors("ShopNet");

            app.UseRouting();

            #region 静态文件
            app.UseStaticFiles(new StaticFileOptions
            {
                //静态资源存储路径
                FileProvider = new PhysicalFileProvider(Directory.GetCurrentDirectory() + "/wwwroot/Images"),
                //静态资源获取路径
                RequestPath = "/wwwroot/Images"
            });
            #endregion
            
            app.UseAuthorization();

            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        

        
    }
}
