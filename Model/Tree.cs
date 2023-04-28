using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    
    public class Tree
    {
        public string? value { get; set; }
        public string? label { get; set; }

        public List<Tree>? children { get; set; }


    }
}
