using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace KeanOkta;

// 录制用户真实按键节奏：监听数字键(0-9)按下，记录相邻两次按键的毫秒间隔，
// 录满 5 个间隔（即连按 6 次数字）后停止并回调。不吞键，用户可正常输入。
public class DelayRecorder : IDisposable
{
	private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

	private struct KBDLL
	{
		public int vkCode;
		public int scanCode;
		public int flags;
		public int time;
		public IntPtr extra;
	}

	private const int WH_KEYBOARD_LL = 13;

	private const int WM_KEYDOWN = 256;

	private const int WM_SYSKEYDOWN = 260;

	public int TargetCount = 5;

	public event Action<int> Pressed;

	public event Action<int[]> Completed;

	private IntPtr hhk;

	private HookProc proc;

	private readonly List<int> intervals = new List<int>();

	private DateTime lastPress = DateTime.MinValue;

	private bool finished;

	public DelayRecorder()
	{
		proc = Callback;
		using Process process = Process.GetCurrentProcess();
		using (process.MainModule)
		{
			hhk = SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(null), 0u);
		}
	}

	[DllImport("user32.dll")]
	private static extern IntPtr SetWindowsHookEx(int id, HookProc cb, IntPtr hMod, uint thread);

	[DllImport("user32.dll")]
	private static extern bool UnhookWindowsHookEx(IntPtr hhk);

	[DllImport("user32.dll")]
	private static extern IntPtr CallNextHookEx(IntPtr hhk, int code, IntPtr w, IntPtr l);

	[DllImport("kernel32.dll")]
	private static extern IntPtr GetModuleHandle(string name);

	private bool IsDigit(int vk)
	{
		return (vk >= 0x30 && vk <= 0x39) || (vk >= 0x60 && vk <= 0x69);
	}

	private IntPtr Callback(int nCode, IntPtr wParam, IntPtr lParam)
	{
		if (nCode >= 0 && !finished)
		{
			int msg = wParam.ToInt32();
			if (msg == WM_KEYDOWN || msg == WM_SYSKEYDOWN)
			{
				KBDLL kb = (KBDLL)Marshal.PtrToStructure(lParam, typeof(KBDLL));
				if (IsDigit(kb.vkCode))
				{
					DateTime now = DateTime.Now;
					if (lastPress != DateTime.MinValue)
					{
						int delta = (int)(now - lastPress).TotalMilliseconds;
						if (delta >= 5 && delta <= 3000)
						{
							intervals.Add(delta);
						}
					}
					lastPress = now;
					if (Pressed != null)
					{
						Pressed(intervals.Count);
					}
					if (intervals.Count >= TargetCount)
					{
						finished = true;
						int[] result = intervals.ToArray();
						UnhookWindowsHookEx(hhk);
						hhk = IntPtr.Zero;
						if (Completed != null)
						{
							Completed(result);
						}
					}
				}
			}
		}
		return CallNextHookEx(hhk, nCode, wParam, lParam);
	}

	public void Dispose()
	{
		if (hhk != IntPtr.Zero)
		{
			UnhookWindowsHookEx(hhk);
			hhk = IntPtr.Zero;
		}
	}
}
