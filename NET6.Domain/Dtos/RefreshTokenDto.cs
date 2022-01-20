namespace NET6.Domain.Dtos
{
    /// <summary>
    /// 刷新Token
    /// </summary>
    public class RefreshTokenDto
    {
        /// <summary>
        /// 旧的访问令牌
        /// </summary>
        public string AccessToken { get; set; }
        /// <summary>
        /// 刷新令牌
        /// </summary>
        public string RefreshToken { get; set; }
    }
}
