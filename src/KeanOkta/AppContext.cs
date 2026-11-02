using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace KeanOkta;

public class AppContext : ApplicationContext
{
	public Settings Settings;

	public string Seed;

	private KeyboardHook hook;

	private NotifyIcon tray;

	private Timer trayTimer;

	private MainForm main;

	private FloatForm floatForm;

	public static Icon Icon
	{
		get
		{
			try
			{
				Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("app.ico");
				if (manifestResourceStream != null)
				{
					return new Icon(manifestResourceStream);
				}
			}
			catch
			{
			}
			return SystemIcons.Application;
		}
	}

	public bool FloatOpen => floatForm != null && !floatForm.IsDisposed && floatForm.Visible;

	public AppContext(bool trayOnly)
	{
		Settings = Store.LoadSettings();
		Seed = Store.LoadSeed();
		tray = new NotifyIcon();
		tray.Icon = Icon;
		tray.Visible = true;
		tray.Text = "Okta Verify";
		BuildMenu();
		tray.MouseClick += delegate(object s, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				ShowPopup();
			}
		};
		hook = new KeyboardHook(() => Settings.Hotkey);
		hook.HotkeyPressed += delegate
		{
			string text = CurrentCode();
			if (text != null)
			{
				// 用“用户录制的真实按键间隔 + 1~15ms 抖动”逐位键入，更接近真人节奏
				AutoType.TypeStringAsync(text, Settings.DelaySamples);
			}
		};
		trayTimer = new Timer
		{
			Interval = 1000
		};
		trayTimer.Tick += delegate
		{
			UpdateTray();
		};
		trayTimer.Start();
		UpdateTray();
		if (!trayOnly && Seed == null)
		{
			ShowMain();
		}
	}

	private void BuildMenu()
	{
		ContextMenuStrip contextMenuStrip = new ContextMenuStrip
		{
			BackColor = Theme.BgPanel,
			ForeColor = Theme.Text,
			Font = Theme.Ui(9f),
			Renderer = new Theme.BlackGoldRenderer()
		};
		contextMenuStrip.Items.Add("显示主界面", null, delegate
		{
			ShowMain();
		});
		contextMenuStrip.Items.Add("迷你悬浮窗", null, delegate
		{
			if (FloatOpen)
			{
				CloseFloat();
			}
			else
			{
				ShowFloat();
			}
		});
		ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem("开机自启");
		toolStripMenuItem.CheckOnClick = false;
		ToolStripMenuItem toolStripMenuItem2 = toolStripMenuItem;
		toolStripMenuItem2.Checked = AutoStart.IsEnabled();
		toolStripMenuItem2.Click += delegate(object s, EventArgs e)
		{
			bool flag = !AutoStart.IsEnabled();
			AutoStart.Set(flag);
			((ToolStripMenuItem)s).Checked = flag;
		};
		contextMenuStrip.Items.Add(toolStripMenuItem2);
		contextMenuStrip.Items.Add(new ToolStripSeparator());
		contextMenuStrip.Items.Add("退出", null, delegate
		{
			ExitApp();
		});
		tray.ContextMenuStrip = contextMenuStrip;
	}

	public string CurrentCode()
	{
		if (string.IsNullOrEmpty(Seed))
		{
			return null;
		}
		byte[] key = Totp.B32Decode(Seed);
		return Totp.AtCounter(key, Totp.CounterAt(DateTimeOffset.UtcNow));
	}

	private void UpdateTray()
	{
		string text = CurrentCode();
		tray.Text = text ?? "未配置";
	}

	public void CloseFloat()
	{
		if (floatForm != null && !floatForm.IsDisposed)
		{
			floatForm.Close();
		}
	}

	private void ShowPopup()
	{
		if (string.IsNullOrEmpty(Seed))
		{
			ShowMain();
			return;
		}
		PopupForm popupForm = new PopupForm(Seed);
		popupForm.Show();
	}

	public void ShowMain()
	{
		if (main == null || main.IsDisposed)
		{
			main = new MainForm(this);
		}
		main.Show();
		main.BringToFront();
		if (main.WindowState == FormWindowState.Minimized)
		{
			main.WindowState = FormWindowState.Normal;
		}
	}

	public void ShowFloat()
	{
		if (floatForm == null || floatForm.IsDisposed)
		{
			floatForm = new FloatForm(this);
		}
		floatForm.Show();
	}

	// 打开悬浮窗（若未开）并弹出尺寸编辑框 —— 供主界面“最小悬浮窗”按钮双击调用
	public void EditFloatSize()
	{
		if (floatForm == null || floatForm.IsDisposed)
		{
			floatForm = new FloatForm(this);
			floatForm.Show();
		}
		floatForm.BringToFront();
		floatForm.EditSize();
	}

	// 打开“悬浮窗编辑”面板：可实时调整宽高（像素），数字等比例缩放，改动即时反映到悬浮窗
	public void ShowFloatEdit()
	{
		ShowFloat();
		floatForm.BringToFront();
		new FloatEditForm(this).ShowDialog(main != null && !main.IsDisposed ? main : (Form)null);
	}

	// “悬浮窗编辑”面板实时改尺寸：把新的宽高作用到当前悬浮窗（字号随之等比缩放）
	public void ResizeFloat(int w, int h)
	{
		if (floatForm != null && !floatForm.IsDisposed)
		{
			floatForm.ResizeTo(w, h);
		}
	}

	// 打开学校 Okta 官网（域名来自设置，默认 sso.kean.edu，可在引导窗修改）
	public void OpenOktaSite()
	{
		string text = (Settings.OrgDomain ?? "").Trim().TrimEnd('/');
		if (text.Length == 0)
		{
			text = "sso.kean.edu";
		}
		int num = text.IndexOf("://");
		if (num >= 0)
		{
			text = text.Substring(num + 3);
		}
		string text2 = "https://" + text;
		try
		{
			Process.Start(text2);
		}
		catch
		{
			try
			{
				Process.Start(new ProcessStartInfo(text2)
				{
					UseShellExecute = true
				});
			}
			catch
			{
			}
		}
	}

	public void OnSeedChanged()
	{
		Seed = Store.LoadSeed();
		UpdateTray();
	}

	public void ExitApp()
	{
		tray.Visible = false;
		hook.Dispose();
		tray.Dispose();
		ExitThread();
	}
}
