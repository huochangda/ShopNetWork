using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NLog.Extensions.Logging;
using NLog.Web;
using RabbitMQ.Client;
using ShopNet.Core;
using ShopNetWork.Extensions;
using ShopNetWork.Filter;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Yitter.IdGenerator;
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

            YitIdHelper.SetIdGenerator(new IdGeneratorOptions());

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
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "ShopNetWork", Version = "v1" });
                //����ע����ʾswagge ui����
                var xmlfile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlpath = Path.Combine(AppContext.BaseDirectory, xmlfile);
                c.IncludeXmlComments(xmlpath, true);
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

            #endregion swagger ����

            #region SqlSugar

            // ��� SqlSugar
            services.AddSqlsugarSetup();

            #endregion SqlSugar

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

            #endregion ����

            #region ��־����

            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                loggingBuilder.AddNLog();
            });

            #endregion ��־����

            #region jwt����

            services.AddAuthentication(options =>
            {
                //������DI������������֤���񣬲���Ĭ�ϵ������֤��������ΪJwtBearerDefaults.AuthenticationScheme
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>//AddJwtBearer()��������JwtBearerOptions
            {
                x.RequireHttpsMetadata = false; //�Ƿ�Ҫʹ��HTTPS��x.RequireHttpsMetadata��
                x.SaveToken = true;//�������ƣ�x.SaveToken��
                x.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {//������֤������x.TokenValidationParameters��
                    ValidateIssuerSigningKey = true,//�Ƿ���֤���з�ǩ����Կ
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(TokenConfig.secret)),//�캯��������һ���µ�SymmetricSecurityKeyʵ������Կ������֤JWT��ǩ��
                    ValidIssuer = TokenConfig.issuer,//����JWT��Ч�ķ��з���Issuer��
                    ValidAudience = TokenConfig.audience,//����JWT�����ߣ�Audience������Чֵ�����Խ���������֤JWT�Ľ������Ƿ���TokenConfig.audience������ָ����ֵ��ƥ�䡣
                    ValidateIssuer = false,//ָʾ�Ƿ���֤JWT�ķ��з�
                    ValidateAudience = true//ָʾ�Ƿ���֤JWT�Ľ��շ�
                };
                x.Events = new JwtBearerEvents()
                {
                    OnMessageReceived = context =>//ΪJWT�����֤����������һ���¼�ί�У��Դ�HTTP������Ϣ����ȡJWT���Ʋ�����洢��HttpContext�У��Ա����ʹ�á�
                    {
                        context.Token = context.Request.Query["access_token"];//��HTTP����Ĳ�ѯ�ַ����л�ȡ��Ϊaccess_token��JWT���ƣ�������洢��HttpContext�е�Token�����С�
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>//ΪJWT�����֤����������һ���¼�ί�У��Դ��������֤ʧ�ܵ����������׳�SecurityTokenExpiredException�쳣����"Token-Expired"��ӵ���Ӧͷ�б�ʾ�����ѹ��ڡ�
                    {
                        // ������ڣ����<�Ƿ����>��ӵ�������ͷ��Ϣ��
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Add("Token-Expired", "true");
                        }
                        return Task.CompletedTask;//����һ������ɵ�������ָʾ�¼�����ɴ���
                    }
                };
            });

            #endregion jwt����
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IConsumer consumer)
        {
            #region jwt

            consumer.Receive();
            //consumer.Stop();
            //��Configure�����Ȩ�ͼ�Ȩ�������
            app.UseAuthentication();//������֤
            app.UseAuthorization();//������Ȩ

            #endregion jwt

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

            #endregion ��̬�ļ�

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}