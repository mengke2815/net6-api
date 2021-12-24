using System.Text;

namespace NET6.Infrastructure.Tools
{
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
        public static object? GetDefaultVal(string typename)
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
            for (int i = 0; i < array.Length; i++)
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
        #region 文件操作
        public static FileInfo[] GetFiles(string directoryPath)
        {
            //如果目录不存在，则抛出异常
            if (!IsExistDirectory(directoryPath))
            {
                throw new DirectoryNotFoundException();
            }
            var root = new DirectoryInfo(directoryPath);
            //获取文件列表
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
    }
}
