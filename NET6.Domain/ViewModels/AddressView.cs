namespace NET6.Domain.ViewModels
{
    public class AddressView
    {
        /// <summary>
        /// 编号
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 收件人姓名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 收件人手机号
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// 省份
        /// </summary>
        public string Province { get; set; }
        /// <summary>
        /// 城市
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// 区域
        /// </summary>
        public string Area { get; set; }
        /// <summary>
        /// 详细地址
        /// </summary>
        public string Detail { get; set; }
        /// <summary>
        /// 是否是默认地址
        /// </summary>
        public bool IsDefault { get; set; }
    }
}
