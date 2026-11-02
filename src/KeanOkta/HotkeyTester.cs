using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace KeanOkta;

public class HotkeyTester : NativeWindow
{
	private const int MOD_ALT = 1;

	private const int MOD_CONTROL = 2;

	private const int MOD_SHIFT = 4;

	private const int MOD_WIN = 8;

	[DllImport("user32.dll")]
	private static extern bool RegisterHotKey(IntPtr h, int id, int fsModifiers, int vk);

	[DllImport("user32.dll")]
	private static extern bool UnregisterHotKey(IntPtr h, int id);

	public static bool CheckFree(HotkeySpec spec, out string problem)
	{
		problem = null;
		if (spec == null || spec.Vk == 0)
		{
			problem = "未指定按键";
			return false;
		}
		if (!spec.Ctrl && !spec.LShift && !spec.RShift && !spec.Alt && !spec.Win)
		{
			problem = "请至少包含一个修饰键（Ctrl/Shift/Alt/Win），否则极易误触";
			return false;
		}
		int fsModifiers = (spec.Ctrl ? 2 : 0) | ((spec.LShift || spec.RShift) ? 4 : 0) | (spec.Alt ? 1 : 0) | (spec.Win ? 8 : 0);
		HotkeyTester hotkeyTester = new HotkeyTester();
		hotkeyTester.CreateHandle(new CreateParams());
		bool flag;
		try
		{
			flag = RegisterHotKey(hotkeyTester.Handle, 1, fsModifiers, spec.Vk);
			if (flag)
			{
				UnregisterHotKey(hotkeyTester.Handle, 1);
			}
		}
		finally
		{
			hotkeyTester.DestroyHandle();
		}
		if (!flag)
		{
			problem = "该组合键可能已被系统或其他程序注册占用，请更换一个再保存";
			return false;
		}
		return true;
	}
}
