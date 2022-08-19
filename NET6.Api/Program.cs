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
var dbtype = DbType.SqlServer;
if (_config.GetConnectionString("SugarConnectDBType") == "mysql")
{
    dbtype = DbType.MySql;
}
builder.Services.AddSingleton(options =>
{
    return new SqlSugarScope(new List<ConnectionConfig>()
    {
        new ConnectionConfig() { ConfigId = DBEnum.默认数据库, ConnectionString = _config.GetConnectionString("SugarConnectString"), DbType = dbtype, IsAutoCloseConnection = true }
    });
});
#endregion

#region 初始化Redis
RedisHelper.Initialization(new CSRedisClient(_config.GetConnectionString("CSRedisConnectString")));
#endregion

#region 添加swagger注释
if (_config["UseSwagger"].ToBool())
{
    builder.Services.AddSwaggerGen(a =>
    {
        a.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = "v1",
            Title = "Api",
            Description = "Api接口文档"
        });
        foreach (var item in groups)
        {
            a.SwaggerDoc(item.Item1, new OpenApiInfo { Version = item.Item1, Title = item.Item2, Description = $"{item.Item2}接口文档" });
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
        {{
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
}
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
builder.Host.UseSerilog((builderContext, config) =>
{
    config
    .MinimumLevel.Warning()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(Path.Combine("Logs", @"Log.txt"), rollingInterval: RollingInterval.Day);
});
#endregion

#region 允许服务器同步IO
builder.Services.Configure<KestrelServerOptions>(a => a.AllowSynchronousIO = true)
                .Configure<IISServerOptions>(a => a.AllowSynchronousIO = true);
#endregion

#region 初始化Autofac 注入程序集
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
var hostBuilder = builder.Host.ConfigureContainer<ContainerBuilder>(builder =>
{
    var assembly = Assembly.Load("NET6.Infrastructure");
    builder.RegisterAssemblyTypes(assembly).Where(a => a.Name.EndsWith("Repository")).AsSelf();
});
#endregion

#region 初始化AutoMapper 自动映射
var serviceAssembly = Assembly.Load("NET6.Domain");
builder.Services.AddAutoMapper(serviceAssembly);
#endregion

#region 注入后台服务
builder.Services.AddHostedService<TimerService>();
#endregion

#region 注入事件总线
builder.Services.AddEventBus(builder =>
{
    builder.ChannelCapacity = 5000;
    builder.AddSubscriber<LoginSubscriber>();
    builder.UnobservedTaskExceptionHandler = (obj, args) =>
    {
        Log.Error($"事件总线异常：{args.Exception}");
    };
    //builder.ReplaceStorer(serviceProvider =>
    //{
    //    return new RedisEventSourceStorer();
    //});
});
#endregion

#region 注入系统缓存
builder.Services.AddMemoryCache();
#endregion

#region 注入http上下文
builder.AddServiceProvider();
#endregion

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<LogActionFilter>();
    options.Filters.Add<GlobalExceptionFilter>();
}).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.Converters.Add(new DatetimeJsonConverter());
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

var app = builder.Build();

// Configure the HTTP request pipeline.

#region 启用静态资源访问
//创建目录
var path = Path.Combine(basePath, "Files/");
CommonFun.CreateDir(path);
//添加MIME支持
var provider = new FileExtensionContentTypeProvider();
provider.Mappings.Add(".fbx", "application/octet-stream");
provider.Mappings.Add(".obj", "application/octet-stream");
provider.Mappings.Add(".mtl", "application/octet-stream");
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
if (_config["UseSwagger"].ToBool())
{
    app.UseSwagger();
    app.UseSwaggerUI(a =>
    {
        a.SwaggerEndpoint("/swagger/v1/swagger.json", "V1 Docs");
        foreach (var item in groups)
        {
            a.SwaggerEndpoint($"/swagger/{item.Item1}/swagger.json", item.Item2);
        }
        a.RoutePrefix = string.Empty;
        a.DocExpansion(DocExpansion.None);
        a.DefaultModelsExpandDepth(-1);//不显示Models
    });
}
#endregion

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
