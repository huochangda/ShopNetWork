using Microsoft.Extensions.Logging;
using Model;
using Model.Dto;
using Model.Shop;
using Newtonsoft.Json;
using ShopNet.Core;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ShopNetWork.Interfaces
{
    /// <summary>
    /// 用户服务
    /// </summary>
    public class UserServices:BaseService<User>,IUserServices,IScopeDependency
    {
        private readonly RedisCache cacheService;//缓存
        private readonly IUnitOfWork unitOfWork;//sqlsugar事务
        public UserServices(ISqlSugarClient db, RedisCache cacheService,
            IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            this.cacheService = cacheService;
            this.unitOfWork = unitOfWork;

        }

        public object UserList(UserQueryDto parm)
        {
            var predicate = Expressionable.Create<User>();
            ///表达式树
            predicate = predicate.And(m => m.IsOpen == true);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.username), m => m.UserName.Contains(parm.username));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.truename), m => m.TrueName.Contains(parm.truename));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.address), m => m.UserAddress.Contains(parm.address));
            predicate = predicate.AndIF(parm.rolename>0, m => m.UserRoleId==parm.rolename);
            PagedInfo<User> page = new PagedInfo<User>();
            int totalCount = 0, totalPages = 0;
            var a = Db.Queryable<User>().Includes(a => a.Role).Where(predicate.ToExpression())
                .OrderByIF(!string.IsNullOrEmpty(parm.Sort), $"{parm.OrderBy} {(parm.Sort == "descending" ? "desc" : "asc")}");
            page.PageSize = parm.PageSize;
            page.PageIndex = parm.PageIndex;
            page.TotalCount = a.Count();
            page.TotalPages = (int)Math.Ceiling(a.Count()*1.0/page.PageSize);
            page.DataSource = a.Skip((page.PageIndex-1)*page.PageSize).Take(page.PageSize).ToList();
            return page;
            // 遍历List<T>并添加查询条件
            // response.DataSource.ForEach(item => {
            //     queryable = queryable.Where(t => t.UserId == item.UserId);
            // });
            //response.DataSource= queryable.Includes(a=>a.Role).ToList();
            //  cacheService.Add("string1", JsonConvert.SerializeObject(response));
            //var pr=JsonConvert.DeserializeObject<PagedInfo<User>>(cacheService.Get<string>("string1"));
            //bool tr = pr.Equals(response);
        }


        /// <summary>
        /// 删除用户假删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>


        public object DeleteUser(int id = 0)
        {
            unitOfWork.BeginTran();
            if (id <= 0)
            {
                return -1;
            }
            var a = GetFirst(a => a.UserId == id);
            a.IsOpen = false;
            var respose = Update(a);
            unitOfWork.CommitTran();
            return respose;
        }




        /// <summary>
        /// 获取用户登录次数统计
        /// </summary>
        /// <returns></returns>
        public object GetLoginLog()
        {
            //var predicate = db.Queryable<LoginLog>().ToList().GroupBy(l => new { l.LoginTime.Year, l.LoginTime.Month });
            //var countList = predicate.Select(l => new
            //{
            //    yearMonth=$"{l.Key.Year}-{l.Key.Month}-{l.Key.Month.ToString("00")}",
            //    total=l.Count()
            //});
            var countList = Db.Queryable<LoginLog>()
                  .ToList()
                  .GroupBy(l => new { l.LoginTime.Year, l.LoginTime.Month })
                  .Select(g => new
                  {
                      YearMonth = $"{g.Key.Year}-{g.Key.Month.ToString("00")}",
                      Count = g.Count()
                  })
                  .ToList();
            return countList;
        }

        /// <summary>
        /// 获取用户年龄姓名统计
        /// </summary>
        /// <returns></returns>
        public object GetAgeStatistics()
        {
            //var data = db.Queryable<User>().ToList().Where(an => an.IsOpen == true);
            var data1 = Db.Queryable<User>().Where(a => a.IsOpen == true).Includes(a => a.Role).ToList();
            return data1;
        }

        /// <summary>
        /// 获取用户地址下拉列表
        /// </summary>
        /// <returns></returns>
        public object GetAddressList()
        {
            var data = Db.Queryable<User>().ToList().GroupBy(a => new { a.UserAddress });
            return data;
        }

        /// <summary>
        /// 获取角色下拉列表
        /// </summary>
        /// <returns></returns>
        public object GetRoleList()
        {
            var data = Db.Queryable<Role>().ToList();
            return data;
        }

        /// <summary>
        /// 批量假删除
        /// </summary>
        /// <returns></returns>
        public object GetRoleList(string ids)
        {
            var data = Db.Queryable<User>().ToList().Where(a => ids.Contains(a.UserId.ToString()));
            foreach (var item in data)
            {
                item.IsOpen = false;
            }
            data = data;
            var respose = Update(data.ToList());
            return data;
        }

        /// <summary>
        /// 批量修改状态
        /// </summary>
        /// <returns></returns>
        public Object AllUpdateState(string ids, bool isopen)
        {
            var data = Db.Queryable<User>().ToList().Where(a => ids.Contains(a.UserId.ToString()));
            foreach (var item in data)
            {
                item.state = isopen;
            }
            data = data;
            var respose = Update(data.ToList());
            return data;
        }
    }
}
