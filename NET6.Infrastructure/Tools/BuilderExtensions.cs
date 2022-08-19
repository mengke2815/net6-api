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
}
