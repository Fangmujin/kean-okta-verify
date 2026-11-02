using System;
using System.Windows.Forms;

namespace KeanOkta;

internal static class Program
{
	[STAThread]
	private static void Main(string[] args)
	{
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(defaultValue: false);
		bool trayOnly = Array.Exists(args, (string a) => a.Equals("--tray", StringComparison.OrdinalIgnoreCase));
		AppContext context = new AppContext(trayOnly);
		Application.Run(context);
	}
}
