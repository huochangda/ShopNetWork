using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Model.Shop
{
    public class GoodType
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int GoodTypeId { get; set; }
        public string TypeName { get; set; }

        public int Pid { get; set; }
    }
}
