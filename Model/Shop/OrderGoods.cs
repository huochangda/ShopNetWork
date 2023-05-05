using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Model.Shop
{
    public class OrderGoods
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int OGId { get; set; }

        public int OrderId { get; set; }

        public int GoodId { get; set; }

        public int BuyCount { get; set; }
    }
}