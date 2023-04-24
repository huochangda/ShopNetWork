using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Model;
using Model.Shop;
using ShopNetWork.Interfaces;
using SqlSugar;

namespace ShopNetWork.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DbHandleController : ControllerBase
    {
        private readonly ISqlSugarClient db;
        /// <summary>
        /// 用户服务接口
        /// </summary>

        public DbHandleController(ISqlSugarClient db)
        {
            this.db = db;
        }

        [HttpGet("CreateDtabase")]
        public IActionResult CreateDtabase()
        {
            db.DbMaintenance.CreateDatabase(); // 注意 ：Oracle和个别国产库需不支持该方法，需要手动建库 
            db.CodeFirst.InitTables(typeof(Role));
            db.CodeFirst.InitTables(typeof(LoginLog));
            //创建表：根据实体类CodeFirstTable1  (所有数据库都支持)    
            db.CodeFirst.InitTables(typeof(User));
            return Ok(new
            {
                ChengCount= 0,
            });
        }
    }
}
