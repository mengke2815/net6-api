namespace NET6.Domain.Dtos
{
    /// <summary>
    /// 登录信息
    /// </summary>
    public class LoginDto
    {
        /// <summary>
        /// 登录名
        /// </summary>
        public string LoginName { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }
    }
}
