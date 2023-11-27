namespace NET.Api.Controllers;

/// <summary>
/// 支付相关
/// </summary>
[Route("payment")]
public class PaymentController : BaseController
{
    readonly IMapper _mapper;
    public PaymentController(IMapper mapper)
    {
        _mapper = mapper;
    }

    /// <summary>
    /// 支付
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost("pay")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IActionResult> PayAsync(BillPayDto dto)
    {
        if (dto.PayType == PayTypeEnum.微信)
        {
            var sign = WeChatTools.GetWeChatPaySign(dto.BillId, 15.8m, "openid");
            return JsonView(true, (object)sign);
        }
        else if (dto.PayType == PayTypeEnum.支付宝)
        {
            return JsonView("暂不支持");
        }
        else
        {
            return JsonView("暂不支持");
        }
    }

    /// <summary>
    /// 微信支付回调
    /// </summary>
    /// <returns></returns>
    [HttpPost("wxnotify")]
    [AllowAnonymous]
    public async Task<IActionResult> WXNotifyAsync()
    {
        var content = new StreamReader(Request.Body).ReadToEnd();
        var notifyData = new WxPayData();
        notifyData.FromXml(content.ToString(), AppSettingsHelper.Get("ApiKey"));
        var res = new WxPayData();
        if (!notifyData.IsSet("transaction_id"))
        {
            res.SetValue("return_code", "FAIL");
            res.SetValue("return_msg", "支付结果中微信订单号不存在");
            return Ok(res.ToXml());
        }
        var transaction_id = notifyData.GetValue("transaction_id").ToString();
        if (!WeChatTools.OrderQuery(transaction_id))
        {
            res.SetValue("return_code", "FAIL");
            res.SetValue("return_msg", "订单查询失败");
            return Ok(res.ToXml());
        }
        if (notifyData.GetValue("result_code").Equals("SUCCESS"))
        {
            var out_trade_no = notifyData.GetValue("out_trade_no").ToString();
            #region 业务逻辑写在此处

            #endregion
            res.SetValue("return_code", "SUCCESS");
            res.SetValue("return_msg", "OK");
            return Ok(res.ToXml());
        }
        else
        {
            res.SetValue("return_code", "FAIL");
            res.SetValue("return_msg", "支付失败");
            return Ok(res.ToXml());
        }
    }
}
