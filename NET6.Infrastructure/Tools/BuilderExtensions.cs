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
        builder.Services.Configure<IpRateLimitOptions>(config.GetSection("IpRateLimiting"));
        builder.Services.Configure<IpRateLimitPolicies>(config.GetSection("IpRateLimitPolicies"));
        builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
        builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
        builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
        builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
        return builder;
    }
}
