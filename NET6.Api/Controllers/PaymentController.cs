namespace NET6.Api.Controllers;

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
    [HttpPost("wxpay")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IActionResult> WXPayAsync(BillPayDto dto)
    {
        return JsonView(true);
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
