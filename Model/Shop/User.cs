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
        /// <summary>
        /// 主键
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int UserId { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        public string PassWord { get; set; }
        /// <summary>
        /// 手机号
        /// </summary>
        [SugarColumn(IsNullable=true)]
        public string Phone { get; set; }

        /// <summary>
        /// 地址列表
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int AddressId { get; set; }
        /// <summary>
        /// 用户角色编号
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int UserRoleId { get; set; }
        /// <summary>
        /// 角色表外键导航
        /// </summary>
        [ForeignKey("UserRoleId")]
        [Navigate(NavigateType.OneToOne, nameof(UserRoleId))]
        public Role?  Role { get; set; }
        /// <summary>
        /// 逻辑删除
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "1")]
        public bool IsOpen { get; set; } = true;
        /// <summary>
        /// 用户信息编号
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int UserInfoId { get; set; }
        /// <summary>
        /// 性别
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "1")]
        public bool Sex { get; set; }
        /// <summary>
        /// 生日
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "2000-1-1")]
        public DateTime Birth { get; set; }
        /// <summary>
        /// 邮箱
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "926578152@163.com")]
        public string Email { get; set; }
        /// <summary>
        /// 用户详情
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "无")]
        public string Des { get; set; }
        /// <summary>
        /// 年龄
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "15")]
        public int Age { get; set; }
        /// <summary>
        /// 注册时间
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "2023-4-21")]
        public DateTime RegisterTime { get; set; }=DateTime.Now;
        /// <summary>
        /// 状态
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "true")]
        public bool state { get; set; } = true;
        /// <summary>
        /// 居住地址
        /// </summary>
        [SugarColumn(IsNullable = true, DefaultValue="北京")]
        public string UserAddress { get; set; }
        /// <summary>
        /// 真实姓名
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public string TrueName { get; set; }



    }
}
