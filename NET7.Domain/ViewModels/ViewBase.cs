namespace NET7.Domain.ViewModels;

/// <summary>
/// View基类
/// </summary>
public class ViewBase
{
    /// <summary>
    /// 编号
    /// </summary>
    public string Id { get; set; }
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreateTime { get; set; }
}
