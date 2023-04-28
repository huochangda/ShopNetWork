using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Model.Dto
{
    public class UserQueryDto:PageParm
    {
        [Display(Name = "查询字符串")]
        public string username { get; set; }

        public string truename { get; set; }

        public string address { get; set; }

        public int rolename { get; set; }
    }
}
