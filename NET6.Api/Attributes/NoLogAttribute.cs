namespace NET6.Api.Attributes
{
    /// <summary>
    /// 禁用操作日志
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class NoLogAttribute : Attribute
    {

    }
}
