using Model;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopNetWork.Interfaces
{
    public interface IUnitOfWork:IScopeDependency
    {
        void BeginTran();
        void CommitTran();
        void RollbackTran();

        SqlSugarClient CurrentDb();
    }
}
