using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Model.Shop
{
    public class Brand
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int BrandId { get; set; }
        public string BrandName { get; set; }
    }
}
