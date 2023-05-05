using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Model;
using Model.Shop;
using ShopNetWork.Interfaces;
using SqlSugar;

namespace ShopNetWork.Controllers
{
    /// <summary>
    /// sqlsugar操作
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class DbHandleController : ControllerBase
    {
        private readonly ISqlSugarClient db;

        public DbHandleController(ISqlSugarClient db)
        {
            this.db = db;
        }

        /// <summary>
        /// codefirst方法
        /// </summary>
        /// <returns></returns>
        [HttpGet("CreateDtabase")]
        public IActionResult CreateDtabase()
        {
            db.DbMaintenance.CreateDatabase(); // 注意 ：Oracle和个别国产库需不支持该方法，需要手动建库

            //订单
            db.CodeFirst.InitTables(typeof(Order));
            db.CodeFirst.InitTables(typeof(OrderGoods));
            db.CodeFirst.InitTables(typeof(OrderLogistics));
            //商品
            db.CodeFirst.InitTables(typeof(Goods));
            db.CodeFirst.InitTables(typeof(GoodsProp));
            db.CodeFirst.InitTables(typeof(GoodsPropType));
            db.CodeFirst.InitTables(typeof(Brand));
            //用户
            db.CodeFirst.InitTables(typeof(Role));
            db.CodeFirst.InitTables(typeof(GoodType));
            db.CodeFirst.InitTables(typeof(LoginLog));
            //创建表：根据实体类CodeFirstTable1  (所有数据库都支持)
            db.CodeFirst.InitTables(typeof(User));
            return Ok(new
            {
                ChengCount = 0,
            });
        }
    }
}