using Model;
using Model.Dto;
using Model.Shop;
using Newtonsoft.Json;
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
        public GoodTypeServices(IUnitOfWork unitOfWork) : base(unitOfWork)
        {


        }
        
        public List<Goods> GetGoodsList(int pageIndex, int pageSize, out int totalCount,
            out int pageCount, string? gName, int bId, int gtId, string? sDate, string? eDate)
        {
            
            var list = Db.Queryable<Goods>().Includes(a => a.GoodsPropType).Includes(a => a.Brand).Where(a => a.IsOpen == true).ToList();
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
            return list.ToList();
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
