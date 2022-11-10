namespace NET7.Api.Attributes;

/// <summary>
/// 不记录操作日志
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class NoLogAttribute : Attribute
{

}
