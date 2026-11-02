using System;

namespace KeanOkta;

// 种子保存的统一入口：引导窗（二维码识别）与手动粘贴框共用。
// 支持三种输入：
//   1) oktaverify://?t=…&f=…&s=…  令牌（联网向 Okta 换取 sharedSecret，域名取自令牌自身）
//   2) otpauth://totp/…?secret=…  链接（直接取 Base32 密钥）
//   3) 纯 Base32 种子串
// 返回 null 表示成功；否则返回可读的错误信息。
public static class SeedSaver
{
	public static string TrySave(AppContext app, string raw)
	{
		string text = (raw ?? "").Trim();
		if (text.Length == 0)
		{
			return "内容为空";
		}
		string san = Enroll.Sanitize(text);
		string seed;
		if (san.StartsWith("oktaverify://"))
		{
			try
			{
				seed = Enroll.DoEnroll(san);
			}
			catch (Exception ex)
			{
				return "失败：" + ex.Message;
			}
		}
		else if (san.StartsWith("otpauth://"))
		{
			seed = ExtractSecret(san);
			if (seed == null)
			{
				return "无法从 otpauth:// 链接解析出 secret 参数";
			}
		}
		else
		{
			seed = san.ToUpperInvariant();
			try
			{
				if (Totp.B32Decode(seed).Length < 5)
				{
					return "Base32 种子解码后长度异常";
				}
			}
			catch
			{
				return "不是有效的 oktaverify:// 令牌，也不是可解码的 Base32 种子";
			}
		}
		try
		{
			Store.SaveSeed(seed);
			app.OnSeedChanged();
			return null;
		}
		catch (Exception ex)
		{
			return "保存失败：" + ex.Message;
		}
	}

	// 从 otpauth://totp/Issuer:account?secret=XXXX&… 中取 secret
	private static string ExtractSecret(string otpauth)
	{
		int q = otpauth.IndexOf('?');
		if (q < 0)
		{
			return null;
		}
		string[] array = otpauth.Substring(q + 1).Split('&');
		foreach (string text in array)
		{
			int num = text.IndexOf('=');
			if (num < 0)
			{
				continue;
			}
			if (text.Substring(0, num) == "secret")
			{
				string text2 = Uri.UnescapeDataString(text.Substring(num + 1));
				text2 = text2.Replace(" ", "").ToUpperInvariant();
				if (text2.Length >= 4)
				{
					return text2;
				}
			}
		}
		return null;
	}
}
