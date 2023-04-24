using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Model.Shop
{

    public class LoginLog
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int LogId { get; set; }

        public int UserId  { get; set; }

        public DateTime LoginTime { get; set; }

       
    }
}
