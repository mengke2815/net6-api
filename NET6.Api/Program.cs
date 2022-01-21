using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NET6.Api.Filters;
using NET6.Api.Services;
using NET6.Domain.Enums;
using NET6.Infrastructure.Tools;
using Serilog;
using SqlSugar;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
var basePath = AppContext.BaseDirectory;

//引入配置文件
var _config = new ConfigurationBuilder()
                 .SetBasePath(basePath)
                 .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                 .Build();

#region 接口分组
var groups = new List<Tuple<string, string>>
{
    //new Tuple<string, string>("Group1","分组一"),
    //new Tuple<string, string>("Group2","分组二")
};
#endregion

#region 注入数据库
builder.Services.AddScoped(options =>
{
    return new SqlSugarClient(new List<ConnectionConfig>()
    {
        new ConnectionConfig() { ConfigId = DBEnum.默认数据库, ConnectionString = _config.GetConnectionString("SugarConnectString"), DbType = DbType.MySql, IsAutoCloseConnection = true }
    });
});
#endregion

#region 初始化Redis
//RedisHelper.Initialization(new CSRedisClient(_config.GetConnectionString("CSRedisConnectString")));
#endregion

#region 添加swagger注释
builder.Services.AddSwaggerGen(a =>
{
    a.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Api"
    });
    foreach (var item in groups)
    {
        a.SwaggerDoc(item.Item1, new OpenApiInfo { Version = item.Item1, Title = item.Item2, Description = "" });
    }
    a.IncludeXmlComments(Path.Combine(basePath, "NET6.Api.xml"), true);
    a.IncludeXmlComments(Path.Combine(basePath, "NET6.Domain.xml"), true);
    a.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Value: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    a.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
      {
        new OpenApiSecurityScheme
        {
          Reference = new OpenApiReference
          {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
          },Scheme = "oauth2",Name = "Bearer",In = ParameterLocation.Header,
        },new List<string>()
      }
    });
});
#endregion

#region 添加身份验证
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidAudience = "net6api.com",
        ValidIssuer = "net6api.com",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSecurityKey"])),
        ClockSkew = TimeSpan.Zero
    };
    //监听JWT过期事件
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers.Add("jwtexception", "expired");
            }
            return Task.CompletedTask;
        }
    };
});
#endregion

#region 初始化日志
Log.Logger = new LoggerConfiguration()
       .MinimumLevel.Warning()
       .Enrich.FromLogContext()
       .WriteTo.Console()
       .WriteTo.File(Path.Combine("Logs", @"Log.txt"), rollingInterval: RollingInterval.Day)
       .CreateLogger();
builder.Host.UseSerilog(Log.Logger, dispose: true);
#endregion

#region 允许服务器同步IO
builder.Services.Configure<KestrelServerOptions>(x => x.AllowSynchronousIO = true)
        .Configure<IISServerOptions>(x => x.AllowSynchronousIO = true);
#endregion

#region 初始化Autofac，注册程序集
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
var hostBuilder = builder.Host.ConfigureContainer<ContainerBuilder>(builder =>
{
    try
    {
        var assemblyServices = Assembly.Load("NET6.Infrastructure");
        builder.RegisterAssemblyTypes(assemblyServices).Where(a => a.Name.EndsWith("Repository")).AsSelf();
    }
    catch (Exception ex)
    {
        throw new Exception(ex.Message + "\n" + ex.InnerException);
    }
});
#endregion

#region 注入后台服务
builder.Services.AddHostedService<TimerService>();
#endregion

#region 注册系统缓存
builder.Services.AddMemoryCache();
#endregion

#region 注册http上下文
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
#endregion

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<LogActionFilter>();
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.Converters.Add(new DatetimeJsonConverter());
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

#region 启用静态资源访问
//创建目录
var path = Path.Combine(basePath, "Files/");
CommonFun.CreateDir(path);
//添加MIME支持
var provider = new FileExtensionContentTypeProvider(new Dictionary<string, string>
{
    {".fbx", "application/octet-stream"},
    {".obj", "application/octet-stream"},
    {".mtl", "application/octet-stream"},
});
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(path),
    ContentTypeProvider = provider,
    RequestPath = "/Files"
});
#endregion

#region 启用跨域访问
app.UseCors(builder => builder
       .WithOrigins(_config["Origins"])
       .AllowCredentials()
       .AllowAnyMethod()
       .AllowAnyHeader());
#endregion

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

#region 启用swaggerUI
app.UseSwagger();
app.UseSwaggerUI(a =>
{
    a.SwaggerEndpoint("/swagger/v1/swagger.json", "V1 Docs");
    foreach (var item in groups)
    {
        a.SwaggerEndpoint($"/swagger/{item.Item1}/swagger.json", item.Item2);
    }
    a.RoutePrefix = string.Empty;
    a.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
    a.DefaultModelsExpandDepth(-1);//不显示Models
});

#endregion

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
