namespace NET6.Infrastructure.Tools;

/// <summary>
/// 微信小程序相关接口
/// </summary>
public class WeChatTools
{
    public static string SessionUrl(string appid, string secret, string code, string grant_type = "authorization_code")
    {
        return string.Format("https://api.weixin.qq.com/sns/jscode2session?appid={0}&secret={1}&js_code={2}&grant_type={3}", appid, secret, code, grant_type);
    }
    public static string AccessTokenUrl(string appid, string secret, string grant_type = "client_credential")
    {
        return string.Format("https://api.weixin.qq.com/cgi-bin/token?appid={0}&secret={1}&grant_type={2}", appid, secret, grant_type);
    }
    public static string PhoneUrl(string access_token)
    {
        return string.Format("https://api.weixin.qq.com/wxa/business/getuserphonenumber?access_token={0}", access_token);
    }
    public static WeChatPhone DecryptedPhone(string encryptedDataStr, string session_key, string iv)
    {
        return CommonFun.ToObject<WeChatPhone>(Decrypted(encryptedDataStr, session_key, iv));
    }
    public static WeChatUser DecryptedUser(string encryptedDataStr, string session_key, string iv)
    {
        return CommonFun.ToObject<WeChatUser>(Decrypted(encryptedDataStr, session_key, iv));
    }

    public static string GetWeChatPaySign(string orderno, decimal total, string openid, string Des = "微信支付", string ClientIp = "127.0.0.1", string FeeType = "CNY")
    {
        var url = "https://api.mch.weixin.qq.com/pay/unifiedorder";
        var nstr = MakeNonceStr();
        var packageParameter = new Hashtable
        {
            { "appid", AppSettingsHelper.Get("WeChatPay:AppId") },
            { "body", Des },
            { "openid", openid },
            { "mch_id", AppSettingsHelper.Get("WeChatPay:MchId") },
            { "notify_url", AppSettingsHelper.Get("Domain") + "/payment/wxnotify" },
            { "nonce_str", nstr },
            { "out_trade_no", orderno },
            { "total_fee", ((int)(total * 100)).ToString() },
            { "spbill_create_ip", ClientIp },
            { "trade_type", "JSAPI" },
            { "fee_type", FeeType }
        };
        var sign = CreateMd5Sign(packageParameter);
        packageParameter.Add("sign", sign);
        var xe = PostDataToWeiXin(url, packageParameter);
        var timeStamp = MakeTimestamp();
        var prepayId = xe.Element("prepay_id").Value;
        nstr = xe.Element("nonce_str").Value;
        var paySignReqHandler = new Hashtable
        {
            { "appId", AppSettingsHelper.Get("WeChatPay:AppId") },
            { "nonceStr", nstr },
            { "signType", "MD5" },
            { "package", "prepay_id=" + prepayId },
            { "timeStamp", timeStamp.ToString() }
        };
        var paySign = CreateMd5Sign(paySignReqHandler);
        var obj = new
        {
            prepayid = prepayId,
            noncestr = nstr,
            timestamp = timeStamp,
            sign = paySign
        };
        return obj.ToJson();
    }

    public static (bool result, string msg) WeChatPayTrans(string openid, double total, string desc = "微信转账", string ClientIp = "127.0.0.1")
    {
        var url = "https://api.mch.weixin.qq.com/mmpaymkttransfers/promotion/transfers";
        var packageParameter = new Hashtable
        {
            { "mch_appid", AppSettingsHelper.Get("WeChatPay:AppId") },
            { "mchid", AppSettingsHelper.Get("WeChatPay:MchId") },
            { "nonce_str", MakeNonceStr() },
            { "openid", openid },
            { "check_name", "NO_CHECK" },
            { "partner_trade_no", CommonFun.GetSerialNumber("R") },
            { "amount", (total * 100).ToString() },
            { "desc", desc },
            { "spbill_create_ip", ClientIp }
        };

        var sign = CreateMd5Sign(packageParameter);
        packageParameter.Add("sign", sign);
        var xe = PostDataToWeiXin(url, packageParameter, true);
        var result_code = xe.Element("result_code").Value;
        if (result_code.Equals("SUCCESS"))
        {
            return (true, "微信转账成功");
        }
        else
        {
            return (false, "微信转账失败");
        }
    }

    public static (bool result, string msg) WeChatPayReturn(string out_trade_no, double total, double refund)
    {
        var url = "https://api.mch.weixin.qq.com/secapi/pay/refund";
        var packageParameter = new Hashtable
        {
            { "appid", AppSettingsHelper.Get("WeChatPay:AppId") },
            { "mch_id", AppSettingsHelper.Get("WeChatPay:MchId") },
            { "nonce_str", MakeNonceStr() },
            { "out_trade_no", out_trade_no },
            { "out_refund_no", CommonFun.GetSerialNumber("R") },
            { "total_fee", ((int)(total * 100)).ToString() },
            { "refund_fee", (refund * 100).ToString() }
        };
        var sign = CreateMd5Sign(packageParameter);
        packageParameter.Add("sign", sign);
        var xe = PostDataToWeiXin(url, packageParameter, true);
        var result_code = xe.Element("result_code").Value;
        if (result_code.Equals("SUCCESS"))
        {
            return (true, "微信退款成功");
        }
        else
        {
            return (false, "微信退款失败");
        }
    }

    #region 微信接口
    private static string Decrypted(string encryptedDataStr, string session_key, string iv)
    {
        var rijalg = Aes.Create();
        rijalg.KeySize = 128;
        rijalg.Padding = PaddingMode.PKCS7;
        rijalg.Mode = CipherMode.CBC;
        rijalg.Key = Convert.FromBase64String(session_key);
        rijalg.IV = Convert.FromBase64String(iv);
        var encryptedData = Convert.FromBase64String(encryptedDataStr);
        //解密 
        var decryptor = rijalg.CreateDecryptor(rijalg.Key, rijalg.IV);
        string result;
        using (var msDecrypt = new MemoryStream(encryptedData))
        {
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt);
            result = srDecrypt.ReadToEnd();
        }
        return result;
    }

    private static string MakeNonceStr()
    {
        var timestap = DateTime.Now.ToString("yyyyMMddhhmmssffff");
        return GetMD5(timestap);
    }
    private static string GetMD5(string src)
    {
        var md5 = new MD5CryptoServiceProvider();
        var data = Encoding.UTF8.GetBytes(src);
        var md5data = md5.ComputeHash(data);
        md5.Clear();
        var retStr = BitConverter.ToString(md5data);
        retStr = retStr.Replace("-", "").ToUpper();
        return retStr;
    }
    private static string CreateMd5Sign(Hashtable parameters)
    {
        var sb = new StringBuilder();
        var akeys = new ArrayList(parameters.Keys);
        akeys.Sort();
        foreach (string k in akeys)
        {
            var v = (string)parameters[k];
            sb.Append(k + "=" + v + "&");
        }
        sb.Append("key=" + AppSettingsHelper.Get("WeChatPay:ApiKey"));
        var sign = GetMD5(sb.ToString());
        return sign;
    }
    private static string getXmlStr(Hashtable parameters)
    {
        var sb = new StringBuilder();
        sb.Append("<xml>");
        foreach (string k in parameters.Keys)
        {
            var v = (string)parameters[k];
            if (Regex.IsMatch(v, @"^[0-9.]$"))
            {
                sb.Append("<" + k + ">" + v + "</" + k + ">");
            }
            else
            {
                sb.Append("<" + k + "><![CDATA[" + v + "]]></" + k + ">");
            }
        }
        sb.Append("</xml>");
        return sb.ToString();
    }
    private static int MakeTimestamp()
    {
        var ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return Convert.ToInt32(ts.TotalSeconds);
    }
    private static XElement PostDataToWeiXin(string url, Hashtable parameters, bool cert = false)
    {
        var xmlStr = getXmlStr(parameters);
        var data = Encoding.UTF8.GetBytes(xmlStr);
        Stream responseStream;
        var request = WebRequest.Create(url) as HttpWebRequest;
        if (cert)
        {
            var cer = new X509Certificate2(AppSettingsHelper.Get("WeChatPay:CerPath"), AppSettingsHelper.Get("WeChatPay:CerPassword"), X509KeyStorageFlags.MachineKeySet);
            request.ClientCertificates.Add(cer);
        }
        request.ContentType = "application/x-www-form-urlencoded";
        request.Method = "POST";
        request.ContentLength = data.Length;
        var requestStream = request.GetRequestStream();
        requestStream.Write(data, 0, data.Length);
        requestStream.Close();
        try
        {
            responseStream = request.GetResponse().GetResponseStream();
        }
        catch (Exception e)
        {
            throw new Exception("响应失败：" + e.Message);
        }
        var str = string.Empty;
        using (var reader = new StreamReader(responseStream, Encoding.UTF8))
        {
            str = reader.ReadToEnd();
        }
        responseStream.Close();
        var xe = XElement.Parse(str);
        return xe;
    }
    public static bool OrderQuery(string transaction_id)
    {
        var url = "https://api.mch.weixin.qq.com/pay/orderquery";
        var packageParameter = new Hashtable
        {
            { "appid", AppSettingsHelper.Get("WeChatPay:AppId") },
            { "mch_id", AppSettingsHelper.Get("WeChatPay:MchId") },
            { "transaction_id", transaction_id },
            { "nonce_str", MakeNonceStr() }
        };
        var sign = CreateMd5Sign(packageParameter);
        packageParameter.Add("sign", sign);
        var xe = PostDataToWeiXin(url, packageParameter);
        var return_code = xe.Element("return_code").Value;
        if (return_code.Equals("SUCCESS"))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    #endregion
}
