using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace KeanOkta;

public static class AutoType
{
	private struct KEYBDINPUT
	{
		public ushort wVk;

		public ushort wScan;

		public uint dwFlags;

		public uint time;

		public IntPtr extra;
	}

	[StructLayout(LayoutKind.Explicit, Size = 40)]
	private struct INPUT
	{
		[FieldOffset(0)]
		public uint type;

		[FieldOffset(8)]
		public KEYBDINPUT ki;
	}

	private const uint INPUT_KEYBOARD = 1u;

	private const uint KEYEVENTF_KEYUP = 2u;

	private const uint KEYEVENTF_SCANCODE = 8u;

	[DllImport("user32.dll", SetLastError = true)]
	private static extern uint SendInput(uint n, INPUT[] inputs, int cbSize);

	[DllImport("user32.dll")]
	private static extern uint MapVirtualKey(uint uCode, uint uMapType);

	private static int CbSize = Marshal.SizeOf(typeof(INPUT));

	private static readonly Random rnd = new Random();

	// 发送一个数字键：使用真实数字键虚拟键码(VK 0x30~0x39) + 对应扫描码，
	// 而不只是 KEYEVENTF_UNICODE 纯字符注入——更接近物理键盘事件，
	// 能通过更多对“合成输入/纯字符注入”做检测的输入框。
	private static void SendDigit(char c)
	{
		if (c < '0' || c > '9')
		{
			return;
		}
		ushort vk = (ushort)(0x30 + (c - '0'));
		ushort scan = (ushort)MapVirtualKey(vk, 0u);
		INPUT[] inputs = new INPUT[2]
		{
			new INPUT
			{
				type = INPUT_KEYBOARD,
				ki = new KEYBDINPUT
				{
					wVk = vk,
					wScan = scan,
					dwFlags = 0u
				}
			},
			new INPUT
			{
				type = INPUT_KEYBOARD,
				ki = new KEYBDINPUT
				{
					wVk = vk,
					wScan = scan,
					dwFlags = KEYEVENTF_KEYUP
				}
			}
		};
		SendInput(2u, inputs, CbSize);
	}

	// 计算下一个位间隔（毫秒）：从录制的 5 个真实间隔里随机取一个，再叠加 1~15ms 随机抖动。
	// 未录制时使用默认基线 45ms。
	private static int NextDelay(int[] samples)
	{
		int baseline = 45;
		if (samples != null && samples.Length > 0)
		{
			baseline = samples[rnd.Next(samples.Length)];
			if (baseline < 5)
			{
				baseline = 5;
			}
			if (baseline > 2000)
			{
				baseline = 2000;
			}
		}
		return baseline + rnd.Next(1, 16);
	}

	// 逐个数字的模拟键盘输入：每按一位都独立下发键下/键上，
	// 位与位之间用“用户录制的真实间隔 + 1~15ms 随机抖动”，模拟真人节奏。
	public static void TypeString(string text, int[] delaySamples)
	{
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		foreach (char c in text)
		{
			if (c >= '0' && c <= '9')
			{
				SendDigit(c);
				Thread.Sleep(NextDelay(delaySamples));
			}
		}
	}

	// 兼容旧签名（无延迟样本）
	public static void TypeString(string text)
	{
		TypeString(text, null);
	}

	// 异步版本：在后台线程逐位键入，避免阻塞 UI 线程与全局键盘钩子。
	public static void TypeStringAsync(string text, int[] delaySamples)
	{
		ThreadStart start = delegate
		{
			try
			{
				TypeString(text, delaySamples);
			}
			catch
			{
			}
		};
		Thread thread = new Thread(start)
		{
			IsBackground = true
		};
		thread.Start();
	}

	public static void TypeStringAsync(string text)
	{
		TypeStringAsync(text, null);
	}
}
