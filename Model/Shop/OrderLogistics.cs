using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Model.Shop
{
    public class OrderLogistics
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int OLId { get; set; }

        public int OrderId { get; set; }

        public string Desc { get; set; }

        public DateTime OperateTime { get; set; }
    }
}