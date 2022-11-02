namespace NET6.Infrastructure.Tools;

/// <summary>
/// Builder扩展
/// </summary>
public static class BuilderExtensions
{
    public static ServiceProvider ServiceProvider { get; set; }
    public static WebApplicationBuilder AddServiceProvider(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        ServiceProvider = builder.Services.BuildServiceProvider();
        return builder;
    }
    public static WebApplicationBuilder AddRateLimit(this WebApplicationBuilder builder, IConfigurationRoot config)
    {
        //加载常规配置
        builder.Services.Configure<IpRateLimitOptions>(config.GetSection("IpRateLimiting"));
        //加载Ip规则限制
        builder.Services.Configure<IpRateLimitPolicies>(config.GetSection("IpRateLimitPolicies"));
        //注入计数器和规则存储
        builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
        builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
        builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
        //配置（解析器、计数器密钥生成器）
        builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
        return builder;
    }
}
