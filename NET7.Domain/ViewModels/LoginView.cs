namespace NET7.Domain.ViewModels;

/// <summary>
/// 登录信息
/// </summary>
public class LoginView
{
    /// <summary>
    /// 访问令牌
    /// </summary>
    public string AccessToken { get; set; }
    /// <summary>
    /// 刷新令牌
    /// </summary>
    public string RefreshToken { get; set; }
    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime Expires { get; set; }
    /// <summary>
    /// 用户Id
    /// </summary>
    public string Id { get; set; }
    /// <summary>
    /// 用户名称
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// 用户头像
    /// </summary>
    public string Avatar { get; set; }
}
