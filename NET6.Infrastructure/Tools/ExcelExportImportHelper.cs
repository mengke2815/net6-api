using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Data;
using System.Reflection;

namespace NET6.Infrastructure.Tools
{
    /// <summary>
    /// Excel导入导出
    /// </summary>
    public class ExcelExportImportHelper
    {
        #region 导出
        /// <summary>
        /// 导出Data到Excel
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="datas">数据实体</param>
        /// <param name="columnNames">列名</param>
        /// <param name="outOfColumn">排除列</param>
        /// <param name="sheetName">sheet名称</param>
        /// <param name="title">标题</param>
        /// <param name="isProtected">是否加密</param>
        /// <returns></returns>
        public static byte[] GetByteToExportExcel<T>(List<T> datas, Dictionary<string, string> columnNames, List<string> outOfColumn, string sheetName = "Sheet1", string title = "", bool isProtected = false)
        {
            using var fs = new MemoryStream();
            using var package = CreateExcelPackage(datas, columnNames, outOfColumn, sheetName, title, isProtected);
            package.SaveAs(fs);
            return fs.ToArray();
        }

        /// <summary>
        /// 创建ExcelPackage
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="datas">数据实体</param>
        /// <param name="columnNames">列名</param>
        /// <param name="outOfColumns">排除列</param>
        /// <param name="sheetName">sheet名称</param>
        /// <param name="title">标题</param>
        /// <param name="isProtected">是否加密</param>
        /// <returns></returns>
        private static ExcelPackage CreateExcelPackage<T>(List<T> datas, Dictionary<string, string> columnNames, List<string> outOfColumns, string sheetName = "Sheet1", string title = "", bool isProtected = false)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add(sheetName);
            if (isProtected)
            {
                worksheet.Protection.IsProtected = true;//设置是否进行锁定
                worksheet.Protection.SetPassword("mima");//设置密码
                worksheet.Protection.AllowAutoFilter = false;//下面是一些锁定时权限的设置
                worksheet.Protection.AllowDeleteColumns = false;
                worksheet.Protection.AllowDeleteRows = false;
                worksheet.Protection.AllowEditScenarios = false;
                worksheet.Protection.AllowEditObject = false;
                worksheet.Protection.AllowFormatCells = false;
                worksheet.Protection.AllowFormatColumns = false;
                worksheet.Protection.AllowFormatRows = false;
                worksheet.Protection.AllowInsertColumns = false;
                worksheet.Protection.AllowInsertHyperlinks = false;
                worksheet.Protection.AllowInsertRows = false;
                worksheet.Protection.AllowPivotTables = false;
                worksheet.Protection.AllowSelectLockedCells = false;
                worksheet.Protection.AllowSelectUnlockedCells = false;
                worksheet.Protection.AllowSort = false;
            }

            var titleRow = 0;
            if (!string.IsNullOrWhiteSpace(title))
            {
                titleRow = 1;
                worksheet.Cells[1, 1, 1, columnNames.Count].Merge = true;//合并单元格
                worksheet.Cells[1, 1].Value = title;
                worksheet.Cells.Style.WrapText = true;
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Style.Font.Size = 12;//字体大小
                worksheet.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;//水平居中
                worksheet.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;//垂直居中
                worksheet.Row(1).Height = 30;//设置行高
                worksheet.Cells.Style.ShrinkToFit = true;//单元格自动适应大小
            }

            //获取要反射的属性,加载首行
            var myType = typeof(T);
            var myPro = new List<PropertyInfo>();
            var i = 1;
            foreach (string key in columnNames.Keys)
            {
                var p = myType.GetProperty(key);
                myPro.Add(p);
                worksheet.Cells[1 + titleRow, i].Value = columnNames[key];
                worksheet.Cells[1 + titleRow, i].Style.Font.Bold = true;
                i++;
            }

            var row = 2 + titleRow;
            foreach (var data in datas)
            {
                var column = 1;
                foreach (PropertyInfo p in myPro.Where(info => !outOfColumns.Contains(info.Name)))
                {
                    worksheet.Cells[row, column].Value = p == null ? "" : Convert.ToString(p.GetValue(data, null));
                    column++;
                }
                row++;
            }
            return package;
        }
        #endregion

        #region 导入
        /// <summary>
        /// Excel到DataTable
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static DataTable? ExcelToDataTable(string filePath)
        {
            try
            {
                var dataTable = new DataTable();
                var file = new FileInfo(filePath);
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var package = new ExcelPackage(file))
                {
                    var worksheet = package.Workbook.Worksheets[0];
                    var rowCount = worksheet.Dimension.Rows;
                    var ColCount = worksheet.Dimension.Columns;
                    //首先添加列
                    for (var col = 1; col <= ColCount; col++)
                    {
                        var dc = new DataColumn(worksheet.Cells[1, col].Value?.ToString());
                        dataTable.Columns.Add(dc);
                    }
                    for (var row = 2; row <= rowCount; row++)
                    {
                        var dataRow = dataTable.NewRow();
                        for (var col = 1; col <= ColCount; col++)
                        {
                            dataRow[col - 1] = worksheet.Cells[row, col].Value?.ToString();
                        }
                        dataTable.Rows.Add(dataRow);
                    }
                }
                return dataTable;
            }
            catch (Exception)
            {
                return null;
            }
        }
        #endregion
    }
}
