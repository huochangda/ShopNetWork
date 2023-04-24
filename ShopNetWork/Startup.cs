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

            RedisServer.Initalize();//����redis�������

            services.AddSingleton<IConnection>(sp =>
            {
                var factory = new ConnectionFactory()
                {
                    HostName = "localhost", // RabbitMQ ������������
                    UserName = "guest",     // RabbitMQ �û���
                    Password = "guest"      // RabbitMQ ����
                };
                return factory.CreateConnection();
            });//rabbit�����۴���ע��

            services.AddScoped<IModel>(sp =>
            {
                var connection = sp.GetRequiredService<IConnection>();
                return connection.CreateModel();
            });//rabbit���д���

            
            services.AddAutoIOC();//�Զ�����ע��
            //services.AddScoped ( typeof(IBaseService<>),typeof(BaseService<>)) ;��������ע��
            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(10); // ����Session����ʱ��Ϊ10����
            });

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => false;//�ر�ŷ��Э��
                options.MinimumSameSitePolicy = SameSiteMode.None;//ʹ��Ĭ�Ϸ���
  
            });

            //services.AddSingleton<MyMiddleware>();�Զ����м��ע��

            services.AddScoped<UserActionFilter>();//�û��������ٹ�����ע��

            //services.AddControllers(options =>
            //{
            //    options.Filters.Add(typeof(UserActionFilter));
            //});ȫ�ֹ�����

            services.AddScoped<RedisCache>();

            #region swagger ����
            //swagger��ӱ���ͷ����������¼���˳���
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "_3315work", Version = "v1" });
                //����ע����ʾswagge ui����
                var xmlfile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlpath = Path.Combine(AppContext.BaseDirectory, xmlfile);
                c.IncludeXmlComments(xmlpath,true);
                //Token�󶨵�configureServices
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,//jwtĬ�ϴ��Authorization��Ϣ��λ��(����ͷ��)
                    Type = SecuritySchemeType.ApiKey,
                    Description = "ֱ�����¿�������Bearer {token}��ע������֮����һ���ո�",
                    Name = "Authorization",//jwtĬ�ϵĲ�������
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
            // ��� SqlSugar
            services.AddSqlsugarSetup();

            #endregion

            #region ����
            services.AddCors(options =>
            {
                options.AddPolicy("ShopNet"//��������
                    , policy =>
                    {
                        policy.SetIsOriginAllowed((host) => true)//�����������
                        .AllowAnyMethod()                       //�����������󷽷�
                        .AllowAnyHeader()                       //�����κε�ͷ����Դ
                        .AllowCredentials()                     //�����κε�����֤��
                        .WithExposedHeaders("Content-Disposition");//
                    });
            });
            #endregion

            #region ��־����
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                loggingBuilder.AddNLog();
            });
            #endregion

            #region jwt����
            services.AddAuthentication(options => {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x => {

                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(TokenConfig.secret)),//��Կ����
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
                        // ������ڣ����<�Ƿ����>��ӵ�������ͷ��Ϣ��
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
            //��Configure�����Ȩ�ͼ�Ȩ�������
            app.UseAuthentication();//������֤
            app.UseAuthorization();//������Ȩ
            #endregion

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ShopNetWork v1"));
                
            }

            app.UseCookiePolicy();//ʹ��cookie����

            app.UseSession();//ʹ��session

            //app.UseRabbitMqReceived();ʹ��rabbitmq�м��

            app.UseCors("ShopNet");

            app.UseRouting();

            #region ��̬�ļ�
            app.UseStaticFiles(new StaticFileOptions
            {
                //��̬��Դ�洢·��
                FileProvider = new PhysicalFileProvider(Directory.GetCurrentDirectory() + "/wwwroot/Images"),
                //��̬��Դ��ȡ·��
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
