using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace KeanOkta;

public class KeyboardHook : IDisposable
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

	private const int WM_KEYUP = 257;

	private const int WM_SYSKEYUP = 261;

	public const int VK_LSHIFT = 160;

	public const int VK_RSHIFT = 161;

	public const int VK_LCONTROL = 162;

	public const int VK_RCONTROL = 163;

	public const int VK_LMENU = 164;

	public const int VK_RMENU = 165;

	public const int VK_LWIN = 91;

	public const int VK_RWIN = 92;

	private IntPtr hhk;

	private HookProc proc;

	private bool triggerDown;

	public Func<HotkeySpec> GetSpec;

	private bool capturing;

	public event Action HotkeyPressed;

	public event Action<HotkeySpec> Captured;

	public KeyboardHook(Func<HotkeySpec> getSpec)
	{
		GetSpec = getSpec;
		proc = Callback;
		using Process process = Process.GetCurrentProcess();
		using (process.MainModule)
		{
			hhk = SetWindowsHookEx(13, proc, GetModuleHandle(null), 0u);
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

	[DllImport("user32.dll")]
	private static extern short GetAsyncKeyState(int vk);

	private static bool Down(int vk)
	{
		return (GetAsyncKeyState(vk) & 0x8000) != 0;
	}

	private IntPtr Callback(int nCode, IntPtr wParam, IntPtr lParam)
	{
		if (nCode >= 0)
		{
			int num = wParam.ToInt32();
			KBDLL kBDLL = (KBDLL)Marshal.PtrToStructure(lParam, typeof(KBDLL));
			bool flag = num == 256 || num == 260;
			bool flag2 = num == 257 || num == 261;
			if (capturing && flag && !IsModifier(kBDLL.vkCode))
			{
				HotkeySpec obj = SpecFromCurrent(kBDLL.vkCode);
				capturing = false;
				if (this.Captured != null)
				{
					this.Captured(obj);
				}
				return new IntPtr(1);
			}
			if (flag || flag2)
			{
				HotkeySpec hotkeySpec = GetSpec();
				if (kBDLL.vkCode == hotkeySpec.Vk)
				{
					if (flag && !triggerDown)
					{
						triggerDown = true;
						if (MatchModifiers(hotkeySpec) && this.HotkeyPressed != null)
						{
							this.HotkeyPressed();
						}
					}
					else if (flag2)
					{
						triggerDown = false;
					}
				}
			}
		}
		return CallNextHookEx(hhk, nCode, wParam, lParam);
	}

	private static bool IsModifier(int vk)
	{
		return vk == 160 || vk == 161 || vk == 162 || vk == 163 || vk == 164 || vk == 165 || vk == 91 || vk == 92;
	}

	private static HotkeySpec SpecFromCurrent(int vk)
	{
		HotkeySpec hotkeySpec = new HotkeySpec();
		hotkeySpec.Ctrl = Down(162) || Down(163);
		hotkeySpec.LShift = Down(160);
		hotkeySpec.RShift = Down(161);
		hotkeySpec.Alt = Down(164) || Down(165);
		hotkeySpec.Win = Down(91) || Down(92);
		hotkeySpec.Vk = vk;
		return hotkeySpec;
	}

	private bool MatchModifiers(HotkeySpec s)
	{
		bool flag = Down(162) || Down(163);
		bool flag2 = Down(160);
		bool flag3 = Down(161);
		bool flag4 = Down(164) || Down(165);
		bool flag5 = Down(91) || Down(92);
		if (s.Ctrl != flag)
		{
			return false;
		}
		if (s.LShift != flag2)
		{
			return false;
		}
		if (s.RShift != flag3)
		{
			return false;
		}
		if (s.Alt != flag4)
		{
			return false;
		}
		if (s.Win != flag5)
		{
			return false;
		}
		return true;
	}

	public void StartCapture()
	{
		capturing = true;
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
