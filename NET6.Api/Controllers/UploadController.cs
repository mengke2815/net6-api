namespace NET6.Api.Controllers;

/// <summary>
/// 文件上传
/// </summary>
[Route("upload")]
public class UploadController : BaseController
{
    readonly IConfiguration _config;
    readonly IWebHostEnvironment _env;
    public UploadController(IConfiguration config, IWebHostEnvironment env)
    {
        _config = config;
        _env = env;
    }

    /// <summary>
    /// 上传文件
    /// </summary>
    /// <param name="path">文件分类的文件夹名称</param>
    /// <returns></returns>
    [HttpPost("file")]
    public async Task<IActionResult> FileUpload(string path = "default")
    {
        var files = Request.Form.Files;
        if (files.Count == 0) return JsonView("请选择文件");

        var domain = _config["Domain"];
        //var dircstr = $"/Files/{path}/{DateTime.Now:yyyyMMdd}/";
        var dircstr = $"/Files/{path}/";
        var result = new List<string>();
        foreach (var file in files)
        {
            var filename = Path.GetFileName(file.FileName);
            if (filename.IsNull()) continue;

            var fileext = Path.GetExtension(filename).ToLower();
            var folderpath = _env.ContentRootPath;
            CommonFun.CreateDir(folderpath + dircstr);
            //重新命名文件
            var pre = DateTime.Now.ToString("yyyyMMddHHmmssffff");
            var after = CommonFun.GetRandom(1000, 9999).ToString();

            var fileloadname = dircstr + pre + "_" + after + ProExt(fileext);
            using (var stream = new FileStream(folderpath + fileloadname, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            result.Add(domain + fileloadname);
        }
        return JsonView(result);
    }

    #region 校验文件类型
    readonly string[] badext = { "exe", "msi", "bat", "com", "sys", "aspx", "asax", "ashx" };
    private string ProExt(string ext)
    {
        if (ext.IsNull()) return "";
        if (badext.Contains(ext)) throw new Exception("危险文件");
        if (ext.First() == '.') return ext;
        return "." + ext;
    }
    #endregion
}
