using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Model.Shop
{
    public class Goods
    {
        /// <summary>
        /// 主键
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int GoodId { get; set; }
        /// <summary>
        /// 商品名称
        /// </summary>
        public string GoodName { get; set; }
       
        /// <summary>
        /// 货号
        /// </summary>
        public string ArtNo { get; set; }
        /// <summary>
        /// 建议价格
        /// </summary>
        [Column(TypeName ="decimal(10,2)")]
        public decimal OfferMoney { get; set; }
        /// <summary>
        /// 市场价
        /// </summary>
        public decimal MarketMoney { get; set; }
        /// <summary>
        /// 库存
        /// </summary>
        public int Num { get; set; }
        /// <summary>
        /// 计量单位
        /// </summary>
        [Column(TypeName = "decimal(10,2)")]
        public string UOM { get; set; }
        /// <summary>
        /// 重量
        /// </summary>
        public decimal Weoght { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 商品图片
        /// </summary>
        public string GoodImages { get; set; }
        /// <summary>
        /// 状态(上架,下架)
        /// </summary>
        public bool GoodState { get; set; } = true;
        /// <summary>
        /// 商品描述富文本
        /// </summary>
        [SugarColumn(Length = 1000)]
        public string GoodDesc { get; set; }
        

        public int GPId { get; set; }

        /// <summary>
        /// 品牌id
        /// </summary>
        public int BrandId { get; set; }
        [ForeignKey("BrandId")]
        [Navigate(NavigateType.OneToOne, nameof(BrandId))]
        public Brand? Brand { get; set; }
        
        /// <summary>
        /// 逻辑删除
        /// </summary>
        public bool IsOpen { get; set; } = true;


        /// <summary>
        /// 商品分类Id
        /// </summary>
        public int GoodTypeId { get; set; }
        /// <summary>
        /// 商品分类所有Id ,逗号分割
        /// </summary>
        public string GoodTypeIdAll { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [NotMapped]
       
        public string? GoodTypeNameAll { get; set; }





        /// <summary>
        /// 商品属性分类Id(下拉列表,存储大小,颜色)
        /// </summary>
        public int GPTId { get; set; }
        [ForeignKey("GPTId")]
        [Navigate(NavigateType.OneToOne, nameof(GPTId))]
        public GoodsPropType? GoodsPropType { get; set; }
        /// <summary>
        /// 商品属性(用,分隔,如:128G,256G,512G)
        /// </summary>
        public string GPContent { get; set; }

    }
}
