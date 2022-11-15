namespace NET7.Domain.Dtos;

/// <summary>
/// 订单支付
/// </summary>
public class BillPayDto
{
    public string BillId { get; set; }
    public PayTypeEnum PayType { get; set; }
}
