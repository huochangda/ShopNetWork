using Model;
using Model.Dto;
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
    public interface IUserServices : IBaseService<User>, IScopeDependency
    {
        object AllUpdateState(string ids, bool isopen);
        object DeleteUser(int id);
        object GetAddressList();
        object GetAgeStatistics();
        object GetLoginLog();
        object GetRoleList(string ids);
        object GetRoleList();
        #region CustomInterface 
        #endregion
        object UserList(UserQueryDto parm);
    }
}
