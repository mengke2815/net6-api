namespace NET6.Domain.ViewModels
{
    /// <summary>
    /// 统一返回结构
    /// </summary>
    public class JsonView
    {
        /// <summary>
        /// 状态码
        /// </summary>
        public int Code { get; set; }
        /// <summary>
        /// 消息
        /// </summary>
        public string Msg { get; set; }
        /// <summary>
        /// 条数
        /// </summary>
        public int Count { get; set; }
        /// <summary>
        /// 数据
        /// </summary>
        public object Data { get; set; }
    }
}
