using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace KeanOkta;

public static class Store
{
	private static string Dir = AppDomain.CurrentDomain.BaseDirectory;

	private static string SeedPath = Path.Combine(Dir, "seed.dat");

	private static string ConfPath = Path.Combine(Dir, "settings.ini");

	private static byte[] Entropy = Encoding.UTF8.GetBytes("KeanOktaTOTP-v1");

	public static string LoadSeed()
	{
		try
		{
			if (!File.Exists(SeedPath))
			{
				return null;
			}
			byte[] encryptedData = File.ReadAllBytes(SeedPath);
			byte[] bytes = ProtectedData.Unprotect(encryptedData, Entropy, DataProtectionScope.CurrentUser);
			return Encoding.UTF8.GetString(bytes);
		}
		catch
		{
			return null;
		}
	}

	public static void SaveSeed(string seed)
	{
		byte[] bytes = ProtectedData.Protect(Encoding.UTF8.GetBytes(seed), Entropy, DataProtectionScope.CurrentUser);
		File.WriteAllBytes(SeedPath, bytes);
	}

	public static Settings LoadSettings()
	{
		Settings settings = new Settings();
		try
		{
			if (File.Exists(ConfPath))
			{
				string[] array = File.ReadAllLines(ConfPath);
				foreach (string text in array)
				{
					int num = text.IndexOf('=');
					if (num >= 0)
					{
						string text2 = text.Substring(0, num).Trim();
						string s = text.Substring(num + 1).Trim();
						switch (text2)
						{
						case "hotkey":
							settings.Hotkey = HotkeySpec.Load(s);
							break;
						case "floatW":
							settings.FloatW = int.Parse(s);
							break;
						case "floatH":
							settings.FloatH = int.Parse(s);
							break;
						case "floatX":
							settings.FloatX = int.Parse(s);
							break;
						case "floatY":
							settings.FloatY = int.Parse(s);
							break;
					case "delaySamples":
						settings.DelaySamples = LoadDelaySamples(s);
						break;
					case "orgDomain":
						if (!string.IsNullOrWhiteSpace(s))
						{
							settings.OrgDomain = s.Trim().TrimEnd('/');
						}
						break;
					}
					}
				}
			}
		}
		catch
		{
		}
		return settings;
	}

	public static void SaveSettings(Settings s)
	{
		List<string> list = new List<string>();
		list.Add("hotkey=" + s.Hotkey.Save());
		list.Add("floatW=" + s.FloatW);
		list.Add("floatH=" + s.FloatH);
		list.Add("floatX=" + s.FloatX);
		list.Add("floatY=" + s.FloatY);
		list.Add("delaySamples=" + SaveDelaySamples(s.DelaySamples));
		list.Add("orgDomain=" + s.OrgDomain);
		List<string> list2 = list;
		File.WriteAllLines(ConfPath, list2.ToArray());
	}

	private static string SaveDelaySamples(int[] samples)
	{
		if (samples == null || samples.Length == 0)
		{
			return "";
		}
		System.Text.StringBuilder sb = new System.Text.StringBuilder();
		for (int i = 0; i < samples.Length; i++)
		{
			if (i > 0)
			{
				sb.Append(',');
			}
			sb.Append(samples[i]);
		}
		return sb.ToString();
	}

	private static int[] LoadDelaySamples(string s)
	{
		if (string.IsNullOrWhiteSpace(s))
		{
			return new int[0];
		}
		try
		{
			string[] array = s.Split(',');
			List<int> list = new List<int>();
			foreach (string t in array)
			{
				int v = int.Parse(t.Trim());
				if (v > 0 && v < 5000)
				{
					list.Add(v);
				}
			}
			return list.ToArray();
		}
		catch
		{
			return new int[0];
		}
	}
}
