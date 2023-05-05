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
    public class Order
    {
        /// <summary>
        /// 订单id
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int OrderId { get; set; }

        public string OrderCode { get; set; }

        public DateTime AddTime { get; set; }

        /// <summary>
        /// 商品数量
        /// </summary>
        public int GoodCount { get; set; }

        /// <summary>
        /// 总金额
        /// </summary>
        [Column(TypeName = "decimal(10,2)")]
        public decimal PayTotal { get; set; }

        public PayType PayType { get; set; }

        public OrderState OrderState { get; set; }

        public bool IsOpen { get; set; } = true;

        public string UserName { get; set; }

        public string Phone { get; set; }

        public string Address { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Freight { get; set; }

        public string ComeFrom { get; set; }
    }

    public enum PayType
    {
        微信 = 1,
        支付宝 = 2,
        银行卡 = 3,
    }

    public enum OrderState
    {
        待付款 = 1,
        待发货 = 2,
        待收货 = 3,
        待评价,
        已关闭,
        退款中,
    }
}