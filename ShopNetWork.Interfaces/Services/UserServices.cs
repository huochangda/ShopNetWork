using Model;
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
    public class UserServices:BaseService<User>,IUserServices
    {
        public UserServices(IUnitOfWork unitOfWork) : base(unitOfWork)
        {

        }
    }
}
