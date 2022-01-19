using SqlSugar;

namespace NET6.Domain.Entities
{
    /// <summary>
    /// 实体基类
    /// </summary>
    public class EntityBase
    {
        /// <summary>
        /// 编号
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnDescription = "主键")]
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        /// <summary>
        /// 是否删除
        /// </summary>
        [SugarColumn(ColumnDescription = "是否删除")]
        public bool IsDeleted { get; set; } = false;
        /// <summary>
        /// 创建者Id
        /// </summary>
        [SugarColumn(ColumnDescription = "创建者Id")]
        public string CreateUserId { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [SplitField]
        [SugarColumn(ColumnDescription = "创建时间")]
        public DateTime CreateTime { get; set; } = DateTime.Now;
        /// <summary>
        /// 删除者Id
        /// </summary>
        [SugarColumn(ColumnDescription = "删除者Id")]
        public string DeleteUserId { get; set; }
        /// <summary>
        /// 删除时间
        /// </summary>
        [SugarColumn(ColumnDescription = "删除时间", IsNullable = true)]
        public DateTime? DeleteTime { get; set; }
    }
}
