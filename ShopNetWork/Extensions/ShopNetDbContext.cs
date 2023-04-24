using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShopNet.Core;
using SqlSugar;
using System;

namespace ShopNetWork.Extensions
{
    public static class ShopNetDbContext
    {
        public static void AddSqlsugarSetup(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddScoped<ISqlSugarClient>(x =>
            {
                return new SqlSugarClient(new ConnectionConfig()
                {
                    ConnectionString = ShopNet.Common.AppSettings.Configuration["ConnectionStrings:Con"],
                    DbType = DbType.SqlServer,
                    IsAutoCloseConnection = true,
                    InitKeyType = InitKeyType.Attribute,
                    ConfigureExternalServices = new ConfigureExternalServices()
                    {
                        DataInfoCacheService = new RedisCache()
                    },
                    MoreSettings = new ConnMoreSettings()
                    {
                        IsAutoRemoveDataCache = true
                    }
                }); ;
            });
        }
    }
}
//public static void AddSqlsugarSetup(this IServiceCollection services)
//{
//    if (services == null) throw new ArgumentNullException(nameof(services));

//    services.AddScoped<ISqlSugarClient>(x =>
//    {
//        return new SqlSugarClient(new ConnectionConfig()
//        {
//            ConnectionString = ShopNet.Common.AppSettings.Configuration["ConnectionStrings:Con"],
//            DbType = DbType.SqlServer,
//            IsAutoCloseConnection = true,
//            InitKeyType = InitKeyType.Attribute,
//            ConfigureExternalServices = new ConfigureExternalServices()
//            {
//                DataInfoCacheService = new RedisCache()
//            },
//            MoreSettings = new ConnMoreSettings()
//            {
//                IsAutoRemoveDataCache = true
//            }
//        }); ;
//    });
//}
