using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Model.Shop
{
    public class GoodsProp
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int GPId { get; set; }
        /// <summary>
        /// 对应分类Id
        /// </summary>
        public string GPName { get; set; }

        public int GPTId { get; set; }
       
    }
}
