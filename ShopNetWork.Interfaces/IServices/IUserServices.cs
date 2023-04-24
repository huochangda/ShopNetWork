using Model;
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
    public interface IUserServices:IBaseService<User>,IScopeDependency
    {
        #region CustomInterface 
        #endregion
    }
}
