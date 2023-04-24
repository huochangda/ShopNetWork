using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Shop
{
    public class Good
    {
        public int GoodId { get; set; }

        public string GoodName { get; set; }

        public int BrandId { get; set; }

        public string ArtNo { get; set; }

        public decimal OfferMoney { get; set; }

        public decimal MarketMoney { get; set; }

        public int Num { get; set; }

        public int UOM { get; set; }

        public decimal Weoght { get; set; }

        public DateTime CreateTime { get; set; }

        public string GoodImages { get; set; }

        public bool GoodState { get; set; }

        public string GoodDesc { get; set; }

        public int MyProperty { get; set; }
    }
}
