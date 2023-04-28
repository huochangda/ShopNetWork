using Model;
using Model.Dto;
using Model.Shop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopNetWork.Interfaces
{
    /// <summary>
    /// 用户接口
    /// </summary>
    public interface IGoodTypeServices : IBaseService<GoodType>, IScopeDependency
    {

        #region CustomInterface 
        #endregion
        List<Tree> GetTree(int pid);

        public List<Goods> GetGoodsList(int pageIndex, int pageSize, out int totalCount,
            out int pageCount, string? gName, int bId, int gtId, string? sDate, string? eDate);
    }
}
