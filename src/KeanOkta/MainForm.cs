using System;
using System.Drawing;
using System.Windows.Forms;

namespace KeanOkta;

public class MainForm : Form
{
	private AppContext app;

	private CodesPanel codes;

	private RichTextBox formula;

	private Label seedState;

	private CheckBox bFloat;

	private Timer timer;

	private Timer formulaTimer;

	private Random rnd = new Random();

	private DateTime lastFormula = DateTime.MinValue;

	public MainForm(AppContext app)
	{
		MainForm mainForm = this;
		this.app = app;
		Text = "Kean Okta Verify · TOTP";
		base.Icon = AppContext.Icon;
		MinimumSize = new Size(640, 600);
		base.Width = 780;
		base.Height = 720;
		base.StartPosition = FormStartPosition.CenterScreen;
		base.MinimizeBox = true;
		base.MaximizeBox = false;
		Theme.ApplyForm(this);
		TableLayoutPanel tableLayoutPanel = new TableLayoutPanel
		{
			Dock = DockStyle.Fill,
			ColumnCount = 1,
			Padding = new Padding(18),
			AutoScroll = true,
			BackColor = Theme.Bg
		};
		tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
		Panel panel = new Panel
		{
			Dock = DockStyle.Top,
			Height = 172,
			Padding = new Padding(0, 4, 0, 4),
			BackColor = Theme.Bg
		};
		codes = new CodesPanel();
		panel.Controls.Add(codes);
		tableLayoutPanel.Controls.Add(panel);
		tableLayoutPanel.RowCount++;
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 172f));
		Panel panel2 = new Panel
		{
			Dock = DockStyle.Top,
			Height = 178,
			Padding = new Padding(0, 6, 0, 6),
			BackColor = Theme.Bg
		};
		formula = new RichTextBox
		{
			Dock = DockStyle.Fill,
			ReadOnly = true,
			DetectUrls = false,
			BackColor = Theme.BgFormula,
			ForeColor = Theme.GoldDim,
			BorderStyle = BorderStyle.FixedSingle,
			Font = Theme.Mono(9f)
		};
		panel2.Controls.Add(formula);
		tableLayoutPanel.Controls.Add(panel2);
		tableLayoutPanel.RowCount++;
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 178f));
		TableLayoutPanel tableLayoutPanel2 = new TableLayoutPanel
		{
			Dock = DockStyle.Top,
			Height = 56,
			ColumnCount = 2,
			BackColor = Theme.Bg
		};
		tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
		tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90f));
		seedState = new Label
		{
			Dock = DockStyle.Fill,
			TextAlign = ContentAlignment.MiddleLeft,
			Font = Theme.Ui(11f),
			ForeColor = Theme.TextDim
		};
		Button button = Theme.GoldButton("编辑", 11f);
		button.Dock = DockStyle.Fill;
		button.Click += EditClick;
		tableLayoutPanel2.Controls.Add(seedState, 0, 0);
		tableLayoutPanel2.Controls.Add(button, 1, 0);
		tableLayoutPanel.Controls.Add(tableLayoutPanel2);
		tableLayoutPanel.RowCount++;
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 56f));
		TableLayoutPanel tableLayoutPanel3 = new TableLayoutPanel
		{
			Dock = DockStyle.Top,
			Height = 116,
			ColumnCount = 3,
			BackColor = Theme.Bg
		};
		for (int i = 0; i < 3; i++)
		{
			tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
		}
		tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
		tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
		Button button2 = Theme.GoldButton("自定义热键");
		button2.Dock = DockStyle.Fill;
		CheckBox bAuto = Theme.GoldToggle("开机自启");
		bAuto.Dock = DockStyle.Fill;
		bAuto.Checked = AutoStart.IsEnabled();
		bFloat = Theme.GoldToggle("最小悬浮窗");
		bFloat.Dock = DockStyle.Fill;
		bFloat.Checked = app.FloatOpen;
		Button btnDelay = Theme.GoldButton("延迟模拟");
		btnDelay.Dock = DockStyle.Fill;
		Button btnFloatEdit = Theme.GoldButton("悬浮窗编辑");
		btnFloatEdit.Dock = DockStyle.Fill;
		Button button3 = Theme.GoldButton("放置到托盘");
		button3.Dock = DockStyle.Fill;
		button2.Click += delegate
		{
			new HotkeyDialog(app).ShowDialog(mainForm);
		};
		bAuto.CheckedChanged += delegate
		{
			AutoStart.Set(bAuto.Checked);
		};
		bFloat.CheckedChanged += delegate
		{
			if (app.FloatOpen != mainForm.bFloat.Checked)
			{
				if (mainForm.bFloat.Checked)
				{
					app.ShowFloat();
				}
				else
				{
					app.CloseFloat();
				}
			}
		};
		// 双击“最小悬浮窗”按钮 → 打开悬浮窗并编辑其尺寸大小
		bFloat.DoubleClick += delegate
		{
			app.EditFloatSize();
		};
		// 延迟模拟 → 录制真实按键节奏，热键录入按其节奏 + 随机抖动
		btnDelay.Click += delegate
		{
			new DelayDialog(app).ShowDialog(mainForm);
		};
		// 悬浮窗编辑 → 实时调整悬浮窗宽 / 高，数字等比例缩放
		btnFloatEdit.Click += delegate
		{
			app.ShowFloatEdit();
		};
		button3.Click += delegate
		{
			mainForm.Hide();
		};
		tableLayoutPanel3.Controls.Add(button2, 0, 0);
		tableLayoutPanel3.Controls.Add(bAuto, 1, 0);
		tableLayoutPanel3.Controls.Add(bFloat, 2, 0);
		tableLayoutPanel3.Controls.Add(btnDelay, 0, 1);
		tableLayoutPanel3.Controls.Add(btnFloatEdit, 1, 1);
		tableLayoutPanel3.Controls.Add(button3, 2, 1);
		tableLayoutPanel.Controls.Add(tableLayoutPanel3);
		tableLayoutPanel.RowCount++;
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 116f));
		base.Controls.Add(tableLayoutPanel);
		timer = new Timer
		{
			Interval = 100
		};
		timer.Tick += Tick;
		// 本地时间 / 公式区：0.3~0.8 秒随机间隔刷新，且仅在主界面可见时运行
		formulaTimer = new Timer();
		formulaTimer.Tick += delegate
		{
			BuildFormula(DateTimeOffset.UtcNow);
			formulaTimer.Interval = rnd.Next(300, 801);
		};
		Tick(null, null);
	}

	private void EditClick(object s, EventArgs e)
	{
		// 打开学校 Okta 官网（域名可配置）+ 弹出置顶引导窗（引导添加验证方式 / 识别二维码）
		app.OpenOktaSite();
		new GuideForm(app).ShowDialog(this);
	}

	private void Tick(object s, EventArgs e)
	{
		DateTimeOffset utcNow = DateTimeOffset.UtcNow;
		double unixFloat = (double)utcNow.ToUnixTimeSeconds() + (double)utcNow.Millisecond / 1000.0;
		if (string.IsNullOrEmpty(app.Seed))
		{
			seedState.Text = "状态：未配置种子（点“编辑”，按引导在官网添加验证方式）";
			codes.Refresh("", utcNow, unixFloat);
		}
		else
		{
			seedState.Text = "状态：种子已配置（DPAPI 加密保存于程序目录 seed.dat）";
			codes.Refresh(app.Seed, utcNow, unixFloat);
		}
		if (bFloat.Checked != app.FloatOpen)
		{
			bFloat.Checked = app.FloatOpen;
		}
	}

	private void BuildFormula(DateTimeOffset now)
	{
		string text = "--------";
		long counter = Totp.CounterAt(now);
		string cur = "";
		int num = 0;
		int num2 = 0;
		string text2 = "";
		if (!string.IsNullOrEmpty(app.Seed))
		{
			Totp.Triple(app.Seed, now, out var _, out cur, out var _, out counter, out var _, out var hash);
			num = hash[19] & 0xF;
			num2 = ((hash[num] & 0x7F) << 24) | (hash[num + 1] << 16) | (hash[num + 2] << 8) | hash[num + 3];
			text2 = BitConverter.ToString(hash).Replace("-", "");
			text = ((app.Seed.Length <= 8) ? app.Seed : (app.Seed.Substring(0, 4) + "****" + app.Seed.Substring(app.Seed.Length - 4)));
		}
		formula.Clear();
		Color c = Theme.GoldBright;
		Color c2 = Theme.TextDim;
		Line("本地时间 = " + DateTime.Now.ToString("HH:mm:ss.fff"), 12f, c, bold: true);
		Line("Unix      = " + now.ToUnixTimeSeconds(), 12f, c, bold: true);
		Line("T         = " + counter, 12f, c, bold: true);
		Line("code      = " + cur, 13f, c, bold: true);
		Line("K  = " + text + " (Base32, 常量)", 8f, c2, bold: false);
		Line("H  = " + text2, 8f, c2, bold: false);
		Line("offset = " + num + "   (H 末字节 & 0x0F)", 8f, c2, bold: false);
		Line("bin = " + num2, 8f, c2, bold: false);
		Line("code = bin mod 10^6   (10^6、30s、HMAC-SHA1 均为恒定)", 8f, c2, bold: false);
		formula.SelectionStart = 0;
		formula.ScrollToCaret();
	}

	private void Line(string text, float size, Color c, bool bold)
	{
		formula.SelectionFont = Theme.Mono(size, bold ? FontStyle.Bold : FontStyle.Regular);
		formula.SelectionColor = c;
		formula.AppendText(text + "\n");
	}

	protected override void OnVisibleChanged(EventArgs e)
	{
		base.OnVisibleChanged(e);
		if (base.Visible)
		{
			timer.Start();
			formulaTimer.Interval = rnd.Next(300, 801);
			formulaTimer.Start();
			Tick(null, null);
			BuildFormula(DateTimeOffset.UtcNow);
		}
		else
		{
			timer.Stop();
			formulaTimer.Stop();
		}
	}

	protected override void OnFormClosing(FormClosingEventArgs e)
	{
		if (e.CloseReason == CloseReason.UserClosing)
		{
			e.Cancel = true;
			Hide();
		}
		base.OnFormClosing(e);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			timer?.Stop();
			formulaTimer?.Stop();
			timer?.Dispose();
			formulaTimer?.Dispose();
		}
		base.Dispose(disposing);
	}
}
