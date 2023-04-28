using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Model.Shop
{
    public class GoodsPropType
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int GPTId { get; set; }
        public string GPTName { get; set; }


    }
}
