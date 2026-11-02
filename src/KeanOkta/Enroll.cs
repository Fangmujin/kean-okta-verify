using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;

namespace KeanOkta;

public static class Enroll
{
	public class Parsed
	{
		public string T;

		public string F;

		public string S;
	}

	private static JavaScriptSerializer Json = new JavaScriptSerializer();

	public static string Sanitize(string text)
	{
		if (text == null)
		{
			return "";
		}
		StringBuilder stringBuilder = new StringBuilder(text.Length);
		foreach (char c in text)
		{
			if (c >= '‐' && c <= '―')
			{
				stringBuilder.Append('-');
			}
			else if (c == '−' || c == '﹣' || c == '－')
			{
				stringBuilder.Append('-');
			}
			else if (!char.IsWhiteSpace(c))
			{
				stringBuilder.Append(c);
			}
		}
		return stringBuilder.ToString();
	}

	public static Parsed Parse(string uri)
	{
		int num = uri.IndexOf('?');
		if (!uri.StartsWith("oktaverify://") || num < 0)
		{
			throw new Exception("不是 oktaverify:// 令牌");
		}
		Parsed parsed = new Parsed();
		string[] array = uri.Substring(num + 1).Split('&');
		foreach (string text in array)
		{
			int num2 = text.IndexOf('=');
			if (num2 >= 0)
			{
				string text2 = text.Substring(0, num2);
				string text3 = text.Substring(num2 + 1);
				switch (text2)
				{
				case "t":
					parsed.T = text3;
					break;
				case "f":
					parsed.F = text3;
					break;
				case "s":
					parsed.S = text3.TrimEnd('/');
					break;
				}
			}
		}
		if (string.IsNullOrEmpty(parsed.T) || string.IsNullOrEmpty(parsed.F) || string.IsNullOrEmpty(parsed.S))
		{
			throw new Exception("令牌缺少 t / f / s 参数");
		}
		return parsed;
	}

	public static string DoEnroll(string rawUri)
	{
		string uri = Sanitize(rawUri);
		Parsed parsed = Parse(uri);
		ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
		string input;
		using (WebClient webClient = new WebClient())
		{
			webClient.Headers[HttpRequestHeader.Accept] = "application/json";
			input = webClient.DownloadString(parsed.S + "/oauth2/v1/keys");
		}
		Dictionary<string, object> dictionary = Json.Deserialize<Dictionary<string, object>>(input);
		ArrayList arrayList = (ArrayList)dictionary["keys"];
		Dictionary<string, object> dictionary2 = (Dictionary<string, object>)arrayList[arrayList.Count - 1];
		Dictionary<string, object> dictionary3 = new Dictionary<string, object>();
		dictionary3.Add("authenticatorId", parsed.F);
		dictionary3.Add("device", new Dictionary<string, object>
		{
			{ "clientInstanceBundleId", "com.okta.android.auth" },
			{ "clientInstanceDeviceSdkVersion", "DeviceSDK 0.19.0" },
			{ "clientInstanceVersion", "6.8.1" },
			{
				"clientInstanceKey",
				new Dictionary<string, object>
				{
					{ "alg", "RS256" },
					{ "e", "AQAB" },
					{ "okta:isFipsCompliant", false },
					{ "okta:kpr", "SOFTWARE" },
					{ "kty", "RSA" },
					{ "use", "sig" },
					{
						"kid",
						(string)dictionary2["kid"]
					},
					{
						"n",
						(string)dictionary2["n"]
					}
				}
			},
			{
				"deviceAttestation",
				new Dictionary<string, object>()
			},
			{ "displayName", "TOTP" + DateTime.Now.ToString("MMdd") },
			{ "fullDiskEncryption", false },
			{ "isHardwareProtectionEnabled", false },
			{ "manufacturer", "unknown" },
			{ "model", "PC" },
			{ "osVersion", "10" },
			{ "platform", "WINDOWS" },
			{ "rootPrivileges", false },
			{ "screenLock", true },
			{ "secureHardwarePresent", false }
		});
		dictionary3.Add("key", "okta_verify");
		dictionary3.Add("methods", new object[1]
		{
			new Dictionary<string, object>
			{
				{ "isFipsCompliant", false },
				{ "supportUserVerification", false },
				{ "type", "totp" }
			}
		});
		Dictionary<string, object> obj = dictionary3;
		byte[] bytes = Encoding.UTF8.GetBytes(Json.Serialize(obj));
		HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(parsed.S + "/idp/authenticators");
		httpWebRequest.Method = "POST";
		httpWebRequest.Accept = "application/json; charset=UTF-8";
		httpWebRequest.ContentType = "application/json; charset=UTF-8";
		httpWebRequest.Headers.Add("Authorization", "OTDT " + parsed.T);
		using (Stream stream = httpWebRequest.GetRequestStream())
		{
			stream.Write(bytes, 0, bytes.Length);
		}
		try
		{
			using HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
			using StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream());
			Dictionary<string, object> dictionary4 = Json.Deserialize<Dictionary<string, object>>(streamReader.ReadToEnd());
			ArrayList arrayList2 = (ArrayList)dictionary4["methods"];
			foreach (Dictionary<string, object> item in arrayList2)
			{
				if (item.TryGetValue("sharedSecret", out var value) && value != null)
				{
					return (string)value;
				}
			}
			throw new Exception("响应中未找到 sharedSecret（该组织可能未开放 TOTP）");
		}
		catch (WebException ex)
		{
			string text = null;
			if (ex.Response != null)
			{
				using StreamReader streamReader2 = new StreamReader(ex.Response.GetResponseStream());
				try
				{
					Dictionary<string, object> dictionary6 = Json.Deserialize<Dictionary<string, object>>(streamReader2.ReadToEnd());
					text = (string)dictionary6["errorSummary"];
				}
				catch
				{
				}
			}
			if (ex.Response is HttpWebResponse { StatusCode: HttpStatusCode.Unauthorized })
			{
				throw new Exception("令牌无效或已过期/已被扫描（401）。请生成“新鲜”令牌、且不要用手机扫描，再试。");
			}
			throw new Exception(text ?? ("注册失败：" + ex.Message));
		}
	}
}
