namespace NET.Infrastructure.Tools;

/// <summary>
/// 工具类
/// </summary>
public static class CommonFun
{
    public static string GUID => Guid.NewGuid().ToString("N");
    public static bool IsNull(this string s)
    {
        return string.IsNullOrWhiteSpace(s);
    }
    public static bool NotNull(this string s)
    {
        return !string.IsNullOrWhiteSpace(s);
    }
    public static int GetRandom(int minNum, int maxNum)
    {
        var seed = BitConverter.ToInt32(Guid.NewGuid().ToByteArray(), 0);
        return new Random(seed).Next(minNum, maxNum);
    }
    public static string GetSerialNumber(string prefix = "")
    {
        return prefix + DateTime.Now.ToString("yyyyMMddHHmmssfff") + GetRandom(1000, 9999).ToString();
    }
    public static string ToJson(this object obj)
    {
        return JsonSerializer.Serialize(obj, new JsonSerializerOptions()
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
        });
    }
    public static T ToObject<T>(this string json)
    {
        return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions()
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
        });
    }
    public static object GetDefaultVal(string typename)
    {
        return typename switch
        {
            "Boolean" => false,
            "DateTime" => default(DateTime),
            "Date" => default(DateTime),
            "Double" => 0.0,
            "Single" => 0f,
            "Int32" => 0,
            "String" => string.Empty,
            "Decimal" => 0m,
            _ => null,
        };
    }
    public static void CoverNull<T>(T model) where T : class
    {
        if (model == null)
        {
            return;
        }
        var typeFromHandle = typeof(T);
        var properties = typeFromHandle.GetProperties();
        var array = properties;
        for (var i = 0; i < array.Length; i++)
        {
            var propertyInfo = array[i];
            if (propertyInfo.GetValue(model, null) == null)
            {
                propertyInfo.SetValue(model, GetDefaultVal(propertyInfo.PropertyType.Name), null);
            }
        }
    }
    public static void CoverNull<T>(List<T> models) where T : class
    {
        if (models.Count == 0)
        {
            return;
        }
        foreach (var model in models)
        {
            CoverNull(model);
        }
    }
    public static bool ToBool(this object thisValue, bool errorvalue = false)
    {
        if (thisValue != null && thisValue != DBNull.Value && bool.TryParse(thisValue.ToString(), out bool reval))
        {
            return reval;
        }
        return errorvalue;
    }

    #region 文件操作
    public static FileInfo[] GetFiles(string directoryPath)
    {
        if (!IsExistDirectory(directoryPath))
        {
            throw new DirectoryNotFoundException();
        }
        var root = new DirectoryInfo(directoryPath);
        return root.GetFiles();
    }
    public static bool IsExistDirectory(string directoryPath)
    {
        return Directory.Exists(directoryPath);
    }
    public static string ReadFile(string Path)
    {
        string s;
        if (!File.Exists(Path))
            s = "不存在相应的目录";
        else
        {
            var f2 = new StreamReader(Path, Encoding.Default);
            s = f2.ReadToEnd();
            f2.Close();
            f2.Dispose();
        }
        return s;
    }
    public static void FileMove(string OrignFile, string NewFile)
    {
        File.Move(OrignFile, NewFile);
    }
    public static void CreateDir(string dir)
    {
        if (dir.Length == 0) return;
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
    }
    #endregion

    #region IP
    /// <summary>
    /// 是否为ip
    /// </summary>
    /// <param name="ip"></param>
    /// <returns></returns>
    public static bool IsIP(string ip)
    {
        return Regex.IsMatch(ip, @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$");
    }
    public static string GetIP(HttpRequest request)
    {
        if (request == null)
        {
            return "";
        }

        var ip = request.Headers["X-Real-IP"].FirstOrDefault();
        if (ip.IsNull())
        {
            ip = request.Headers["X-Forwarded-For"].FirstOrDefault();
        }
        if (ip.IsNull())
        {
            ip = request.HttpContext?.Connection?.RemoteIpAddress?.ToString();
        }
        if (ip.IsNull() || !IsIP(ip))
        {
            ip = "127.0.0.1";
        }

        return ip;
    }
    #endregion

    /// <summary>
    /// 解析xml注释
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static List<string> ParseXml(this ActionExecutingContext context, string xmlName = "")
    {
        var list = new List<string>();
        if (xmlName.IsNull())
        {
            xmlName = Assembly.GetEntryAssembly().GetName().Name + ".xml";
        }
        if (File.Exists(Path.Combine(AppContext.BaseDirectory, xmlName)))
        {
            var xml = XElement.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, xmlName)));
            var route = ((ControllerActionDescriptor)context.ActionDescriptor).AttributeRouteInfo.Template.ToLower();
            var method = context.HttpContext.Request.Method.ToLower();
            var cName = ((ControllerActionDescriptor)context.ActionDescriptor).ControllerTypeInfo.FullName;
            var mName = ((ControllerActionDescriptor)context.ActionDescriptor).ActionName;
            var members = xml.Elements().FirstOrDefault(a => a.Name == "members").Elements();
            var param = ((ControllerActionDescriptor)context.ActionDescriptor).Parameters;
            var paramList = new List<string>();
            foreach (var item in param)
            {
                paramList.Add(item.ParameterType.FullName);
            }
            var pms = "";
            if (paramList.Count > 0)
            {
                pms = $"({string.Join(',', paramList)})";
            }
            var cDesc = members.FirstOrDefault(a => a.FirstAttribute.Value == $"T:{cName}")?.Elements().FirstOrDefault(a => a.Name == "summary")?.Value.Trim();
            var mDesc = members.FirstOrDefault(a => a.FirstAttribute.Value == $"M:{cName}.{mName}Async{pms}")?.Elements().FirstOrDefault(a => a.Name == "summary")?.Value.Trim();
            if (mDesc.IsNull())
            {
                mDesc = members.FirstOrDefault(a => a.FirstAttribute.Value == $"M:{cName}.{mName}{pms}")?.Elements().FirstOrDefault(a => a.Name == "summary")?.Value.Trim();
            }
            list.Add(route);
            list.Add(method);
            list.Add(cName);
            list.Add(mName);
            list.Add(cDesc);
            list.Add(mDesc);
        }
        return list;
    }
}
