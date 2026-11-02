using System.Windows.Forms;
using Microsoft.Win32;

namespace KeanOkta;

public static class AutoStart
{
	private const string RUN = "Software\\Microsoft\\Windows\\CurrentVersion\\Run";

	private const string NAME = "KeanOktaTOTP";

	public static bool IsEnabled()
	{
		using RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run");
		return registryKey != null && registryKey.GetValue("KeanOktaTOTP") != null;
	}

	public static void Set(bool on)
	{
		using RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", writable: true);
		if (on)
		{
			string text = Application_ExecutablePath();
			registryKey.SetValue("KeanOktaTOTP", "\"" + text + "\" --tray");
		}
		else if (registryKey.GetValue("KeanOktaTOTP") != null)
		{
			registryKey.DeleteValue("KeanOktaTOTP", throwOnMissingValue: false);
		}
	}

	private static string Application_ExecutablePath()
	{
		return Application.ExecutablePath;
	}
}
