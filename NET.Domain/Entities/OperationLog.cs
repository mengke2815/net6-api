﻿namespace NET.Domain.Entities;

/// <summary>
/// 操作日志-使用自动分表
/// </summary>
[SplitTable(SplitType.Month)]
[SugarTable("operation_log_{year}{month}{day}")]
[Tenant(DBEnum.Default)]
public class OperationLog : EntityBase
{
    /// <summary>
    /// IP
    /// </summary>
    public string IP { get; set; }
    /// <summary>
    /// 浏览器
    /// </summary>
    public string Browser { get; set; }
    /// <summary>
    /// 操作系统
    /// </summary>
    public string Os { get; set; }
    /// <summary>
    /// 设备
    /// </summary>
    public string Device { get; set; }
    /// <summary>
    /// 浏览器信息
    /// </summary>
    [SugarColumn(ColumnDataType = "text")]
    public string BrowserInfo { get; set; }
    /// <summary>
    /// 耗时（毫秒）
    /// </summary>
    public long ElapsedMilliseconds { get; set; }
    /// <summary>
    /// 接口地址
    /// </summary>
    public string ApiPath { get; set; }
    /// <summary>
    /// 接口提交方法
    /// </summary>
    public string ApiMethod { get; set; }
    /// <summary>
    /// 操作参数
    /// </summary>
    [SugarColumn(ColumnDataType = "text")]
    public string Params { get; set; }
    /// <summary>
    /// 操作结果
    /// </summary>
    [SugarColumn(ColumnDataType = "text")]
    public string Result { get; set; }
}
