using System;
using System.Collections.Generic;

namespace KeanOkta;

public class HotkeySpec
{
	public bool Ctrl;

	public bool LShift;

	public bool RShift;

	public bool Alt;

	public bool Win;

	public int Vk = 187;

	public string Display
	{
		get
		{
			List<string> list = new List<string>();
			if (Ctrl)
			{
				list.Add("Ctrl");
			}
			if (LShift)
			{
				list.Add("左Shift");
			}
			if (RShift)
			{
				list.Add("右Shift");
			}
			if (Alt)
			{
				list.Add("Alt");
			}
			if (Win)
			{
				list.Add("Win");
			}
			string item = ((Vk != 187) ? ("键" + Vk.ToString("X")) : "+/=");
			list.Add(item);
			return string.Join(" + ", list.ToArray());
		}
	}

	public static HotkeySpec Default()
	{
		HotkeySpec hotkeySpec = new HotkeySpec();
		hotkeySpec.Ctrl = true;
		hotkeySpec.LShift = true;
		hotkeySpec.Vk = 187;
		return hotkeySpec;
	}

	public string Save()
	{
		List<string> list = new List<string>();
		if (Ctrl)
		{
			list.Add("CTRL");
		}
		if (LShift)
		{
			list.Add("LSHIFT");
		}
		if (RShift)
		{
			list.Add("RSHIFT");
		}
		if (Alt)
		{
			list.Add("ALT");
		}
		if (Win)
		{
			list.Add("WIN");
		}
		list.Add("VK" + Vk.ToString("X2"));
		return string.Join("|", list.ToArray());
	}

	public static HotkeySpec Load(string s)
	{
		HotkeySpec hotkeySpec = new HotkeySpec();
		hotkeySpec.Vk = 187;
		HotkeySpec hotkeySpec2 = hotkeySpec;
		if (string.IsNullOrWhiteSpace(s))
		{
			return Default();
		}
		string[] array = s.Split('|');
		foreach (string text in array)
		{
			switch (text)
			{
			case "CTRL":
				hotkeySpec2.Ctrl = true;
				continue;
			case "LSHIFT":
				hotkeySpec2.LShift = true;
				continue;
			case "RSHIFT":
				hotkeySpec2.RShift = true;
				continue;
			case "ALT":
				hotkeySpec2.Alt = true;
				continue;
			case "WIN":
				hotkeySpec2.Win = true;
				continue;
			}
			if (text.StartsWith("VK"))
			{
				hotkeySpec2.Vk = Convert.ToInt32(text.Substring(2), 16);
			}
		}
		return hotkeySpec2;
	}
}
