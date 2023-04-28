using Microsoft.Extensions.Logging;
using Model;
using Model.Dto;
using Model.Shop;
using Newtonsoft.Json;
using ShopNet.Core;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopNetWork.Interfaces
{
    /// <summary>
    /// 用户服务
    /// </summary>
    public class GoodTypeServices:BaseService<GoodType>,IGoodTypeServices,IScopeDependency
    {
        private readonly RedisCache cacheService;//缓存
        public GoodTypeServices(IUnitOfWork unitOfWork, RedisCache cacheService) : base(unitOfWork)
        {

        this.cacheService = cacheService;
        }
        
        public List<Goods> GetGoodsList(int pageIndex, int pageSize, out int totalCount,
            out int pageCount, string? gName, int bId, int gtId, string? sDate, string? eDate)
        {
            
            
            var list = Db.Queryable<Goods>().Includes(a => a.GoodsPropType).Includes(a => a.Brand).Where(a => a.IsOpen == true).ToList();
            //cacheService.Add<string>("Goods",JsonConvert.SerializeObject(list));
            if (!string.IsNullOrEmpty(gName))
            {
                list = list.Where(g => g.GoodName.Contains(gName)).ToList();
            }
            if (bId > 0)
            {
                list = list.Where(g => g.BrandId == bId).ToList();
            }
            if (gtId > 0)
            {
                list = list.Where(g => g.GoodTypeId == gtId).ToList();
            }
            
            if (!string.IsNullOrEmpty(sDate))
            {
                list = list.Where(g => g.CreateTime >= DateTime.Parse(sDate)).ToList();
            }

            if (!string.IsNullOrEmpty(eDate))
            {
                list = list.Where(g => g.CreateTime < DateTime.Parse(eDate)).ToList();
            }
            totalCount = list.Count();
            pageCount = (int)Math.Ceiling(totalCount * 1.0 / pageSize);
            list = list.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            foreach (var item in list.ToList())
            {
                item.GoodTypeNameAll = this.GetGTNameById(item.GoodTypeIdAll);
            }
            return list.ToList();
        }
        private string GetGTNameById(string ids) // ",1,3,9,"
        {
            
                // 转换字符串为字符数组
                string[] idArr = ids.Split(',', StringSplitOptions.RemoveEmptyEntries); // 拆分字符串为数组，把空值删除
                // 查询Id对应的商品分类列表
                List<GoodType> gtList = Db.Queryable<GoodType>().Where(gt => idArr.Contains(gt.GoodTypeId.ToString())).ToList();
                // 获取分类名称
                List<string> nameList = gtList.Select(gt => gt.TypeName).ToList();

                // 转换List为字符串
                string gtName = string.Join("-", nameList);

                return gtName;
            
            
        }


        public List<Tree> GetTree(int pid)
        {
            var list = Db.Queryable<GoodType>().Where(an => an.Pid == pid).ToList();
            if (list.Count == 0)
            {
                return null;
            }
            //转换数据类型
            List<Tree> tree = list.Select(gt => new Tree
            {
                value = gt.GoodTypeId.ToString(),
                label = gt.TypeName,
                children = GetTree(gt.GoodTypeId)
            }).ToList();
            return tree;
        }
    }
}
