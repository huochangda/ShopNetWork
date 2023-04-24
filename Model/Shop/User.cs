using Model.Shop;
using SqlSugar;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model
{
    /// <summary>
    /// 用户模型
    /// </summary>
    [SugarTable("User")]
    public class User 
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int UserId { get; set; }

        public string UserName { get; set; }

        public string PassWord { get; set; }
        [SugarColumn(IsNullable=true)]
        public string Phone { get; set; }


        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int AddressId { get; set; }

        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int UserRoleId { get; set; }
        [ForeignKey("UserRoleId")]
        [Navigate(NavigateType.OneToOne, nameof(UserRoleId))]
        public Role?  Role { get; set; }

        [SugarColumn(IsNullable = false, DefaultValue = "1")]
        public bool IsOpen { get; set; } = true;
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int UserInfoId { get; set; }
        [SugarColumn(IsNullable = false, DefaultValue = "1")]
        public bool Sex { get; set; }
        [SugarColumn(IsNullable = false, DefaultValue = "2000-1-1")]
        public DateTime Birth { get; set; }
        [SugarColumn(IsNullable = false, DefaultValue = "926578152@163.com")]
        public string Email { get; set; }
        [SugarColumn(IsNullable = false, DefaultValue = "无")]
        public string Des { get; set; }
        [SugarColumn(IsNullable = false, DefaultValue = "15")]
        public int Age { get; set; }
        [SugarColumn(IsNullable = false, DefaultValue = "2023-4-21")]
        public DateTime RegisterTime { get; set; }=DateTime.Now;

        [SugarColumn(IsNullable = false, DefaultValue = "true")]
        public bool state { get; set; } = true;
        [SugarColumn(IsNullable = false, DefaultValue="北京", IsOnlyIgnoreInsert=true)]
        public string UserAddress { get; set; }
        [SugarColumn(IsNullable = true)]
        public string TrueName { get; set; }



    }
}
