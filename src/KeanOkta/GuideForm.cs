using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace KeanOkta;

// 置顶悬浮引导窗（白色 60% 透明、可拖动）：
// 点主界面“编辑”后自动弹出，引导用户在学校 Okta 官网添加另一个 TOTP 验证方式，
// 最后一步“识别二维码”会自动截屏解码屏幕上的二维码 → 联网换取种子 → 保存 →
// 关闭引导并弹回主界面开始计算。同时保留“手动粘贴”兜底。
public class GuideForm : Form
{
	private static readonly Color Ink = Color.FromArgb(26, 26, 30);

	private static readonly Color InkDim = Color.FromArgb(80, 80, 88);

	private static readonly Color Ok = Color.FromArgb(22, 110, 60);

	private static readonly Color Bad = Color.FromArgb(176, 32, 32);

	private static readonly Color Info = Color.FromArgb(20, 80, 180);

	private AppContext app;

	private int step;

	private Label counter;

	private Label title;

	private Label desc;

	private Label status;

	private TextBox domainBox;

	private Button nextBtn;

	private Button pickBtn;

	private Button manualBtn;

	private Button skipBtn;

	private Button openSiteBtn;

	private Label closeX;

	private bool dragging;

	private Point dragScreen;

	private readonly string[] Titles = new string[6]
	{
		"① 自行登录官网",
		"② 使用旧的手机版 Okta 验证",
		"③ 右上角「账户设置」",
		"④ 安全方法：「设置另一个」",
		"⑤ 再次使用手机进行安全验证",
		"⑥ 点击下方按钮，识别二维码"
	};

	private readonly string[] Descs = new string[6]
	{
		"在浏览器里打开学校 Okta 官网并登录你的账号。\n网页没自动打开？点下方“打开官网”。",
		"登录时的安全验证，请用你原来手机上 Okta Verify 的推送批准。",
		"登录成功后，点页面右上角的头像 / 账户名，进入「设置 Settings」。",
		"在「安全方法 / 验证方法」里点“设置另一个（Set up another）”，选择 Okta Verify。",
		"添加新验证方式前，Okta 会要求旧手机再验证一次，照常批准即可。",
		"页面弹出的二维码会被本程序自动识别，并联网换取种子。\n识别成功后本窗口关闭，直接弹回程序开始计算验证码。"
	};

	public GuideForm(AppContext app)
	{
		this.app = app;
		Text = "配置引导";
		base.Icon = AppContext.Icon;
		base.Width = 500;
		base.Height = 316;
		base.FormBorderStyle = FormBorderStyle.None;
		base.StartPosition = FormStartPosition.Manual;
		base.TopMost = true;
		base.ShowInTaskbar = false;
		BackColor = Color.White;
		ForeColor = Ink;
		base.Opacity = 0.6;
		Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
		base.Location = new Point(workingArea.Left + (workingArea.Width - base.Width) / 2, workingArea.Top + 28);
		Label label = new Label
		{
			Left = 16,
			Top = 12,
			Width = 120,
			Font = Theme.Ui(9.5f),
			ForeColor = InkDim,
			Text = "学校 Okta 域名："
		};
		domainBox = new TextBox
		{
			Left = 130,
			Top = 9,
			Width = 160,
			Font = Theme.Ui(9.5f),
			Text = app.Settings.OrgDomain,
			BackColor = Color.White,
			ForeColor = Ink,
			BorderStyle = BorderStyle.FixedSingle
		};
		domainBox.Leave += SaveDomain;
		openSiteBtn = FlatBtn("打开官网", 300, 8, 84, 26);
		openSiteBtn.Click += delegate
		{
			SaveDomain(null, null);
			app.OpenOktaSite();
		};
		closeX = new Label
		{
			Left = 464,
			Top = 10,
			Width = 26,
			Height = 24,
			Font = Theme.UiBold(12f),
			ForeColor = InkDim,
			TextAlign = ContentAlignment.MiddleCenter,
			Text = "✕"
		};
		closeX.Click += delegate
		{
			Close();
		};
		counter = new Label
		{
			Left = 16,
			Top = 46,
			Width = 200,
			Font = Theme.Ui(9f),
			ForeColor = InkDim,
			Text = "第 1 / 6 步"
		};
		title = new Label
		{
			Left = 16,
			Top = 70,
			Width = 468,
			Height = 30,
			Font = Theme.UiBold(14f),
			ForeColor = Ink,
			Text = Titles[0]
		};
		desc = new Label
		{
			Left = 16,
			Top = 104,
			Width = 468,
			Height = 76,
			Font = Theme.Ui(10f),
			ForeColor = InkDim,
			Text = Descs[0]
		};
		status = new Label
		{
			Left = 16,
			Top = 186,
			Width = 468,
			Height = 40,
			Font = Theme.Ui(9.5f),
			ForeColor = Info,
			Text = ""
		};
		nextBtn = FlatBtn("完成", 16, 236, 150, 32);
		nextBtn.Click += NextClick;
		pickBtn = FlatBtn("框选识别", 176, 236, 110, 32);
		pickBtn.Visible = false;
		pickBtn.Click += PickClick;
		manualBtn = FlatBtn("手动粘贴", 296, 236, 92, 32);
		manualBtn.Click += delegate
		{
			new SeedDialog(app).ShowDialog(this);
		};
		skipBtn = FlatBtn("跳过引导", 398, 236, 86, 32);
		skipBtn.Click += delegate
		{
			Close();
		};
		base.Controls.AddRange(new Control[12] { label, domainBox, openSiteBtn, closeX, counter, title, desc, status, nextBtn, pickBtn, manualBtn, skipBtn });
		Label[] array = new Label[3] { title, desc, status };
		foreach (Label c in array)
		{
			c.MouseDown += BeginDrag;
			c.MouseMove += OnDrag;
			c.MouseUp += EndDrag;
		}
		base.MouseDown += BeginDrag;
		base.MouseMove += OnDrag;
		base.MouseUp += EndDrag;
	}

	private static Button FlatBtn(string text, int x, int y, int w, int h)
	{
		Button button = new Button
		{
			Text = text,
			Left = x,
			Top = y,
			Width = w,
			Height = h,
			FlatStyle = FlatStyle.Flat,
			BackColor = Color.FromArgb(28, 28, 34),
			ForeColor = Theme.GoldBright,
			Font = Theme.Ui(9.5f),
			Cursor = Cursors.Hand
		};
		button.FlatAppearance.BorderColor = Color.FromArgb(120, 100, 50);
		button.FlatAppearance.BorderSize = 1;
		return button;
	}

	// —— 拖动（用屏幕坐标差，任意控件上按下都能平滑拖动）——
	private void BeginDrag(object s, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			dragging = true;
			dragScreen = Cursor.Position;
		}
	}

	private void OnDrag(object s, MouseEventArgs e)
	{
		if (dragging)
		{
			Point point = Cursor.Position;
			base.Left += point.X - dragScreen.X;
			base.Top += point.Y - dragScreen.Y;
			dragScreen = point;
		}
	}

	private void EndDrag(object s, MouseEventArgs e)
	{
		dragging = false;
	}

	private void SaveDomain(object s, EventArgs e)
	{
		string text = (domainBox.Text ?? "").Trim().TrimEnd('/');
		int num = text.IndexOf("://");
		if (num >= 0)
		{
			text = text.Substring(num + 3);
		}
		if (text.Length == 0)
		{
			text = "sso.kean.edu";
		}
		domainBox.Text = text;
		if (app.Settings.OrgDomain != text)
		{
			app.Settings.OrgDomain = text;
			Store.SaveSettings(app.Settings);
		}
	}

	private void ShowStep()
	{
		counter.Text = "第 " + (step + 1) + " / 6 步";
		title.Text = Titles[step];
		desc.Text = Descs[step];
		nextBtn.Text = ((step == 5) ? "识别二维码" : "完成");
		pickBtn.Visible = false;
		status.ForeColor = Info;
		status.Text = ((step == 5) ? "准备好后点“识别二维码”；识别失败可点“框选识别”手动圈选。" : "");
	}

	private void NextClick(object s, EventArgs e)
	{
		if (step < 5)
		{
			step++;
			ShowStep();
			return;
		}
		DoRecognize(auto: true);
	}

	private void PickClick(object s, EventArgs e)
	{
		DoRecognize(auto: false);
	}

	// auto=true 全屏自动识别；auto=false 框选识别
	private void DoRecognize(bool auto)
	{
		nextBtn.Enabled = false;
		pickBtn.Enabled = false;
		manualBtn.Enabled = false;
		status.ForeColor = Info;
		status.Text = (auto ? "正在识别屏幕上的二维码，请保持二维码完整可见…" : "请在屏幕上拖框圈住二维码…");
		Application.DoEvents();
		QrScanner.Hit hit = null;
		try
		{
			if (auto)
			{
				Hide();
				Application.DoEvents();
				List<QrScanner.Hit> hits = QrScanner.ScanScreen();
				Show();
				if (hits.Count == 0)
				{
					status.ForeColor = Bad;
					status.Text = "未在屏幕上识别到二维码。请确认二维码完整显示、未被本窗口或其它窗口遮挡，然后重试；或点“框选识别”。";
					pickBtn.Visible = true;
					Unlock();
					return;
				}
				if (hits.Count == 1)
				{
					hit = hits[0];
				}
				else
				{
					status.Text = "识别到 " + hits.Count + " 个二维码，请选择…";
					Application.DoEvents();
					hit = QrPickDialog.Pick(hits);
				}
			}
			else
			{
				Hide();
				Application.DoEvents();
				hit = QrScanner.SelectAndScan();
				Show();
			}
		}
		catch (Exception ex)
		{
			Show();
			status.ForeColor = Bad;
			status.Text = "识别过程出错：" + ex.Message;
			Unlock();
			return;
		}
		if (hit == null)
		{
			status.ForeColor = Bad;
			status.Text = (auto ? "你取消了选择。可重新点“识别二维码”，或点“框选识别”。" : "框选区域内没有二维码，或框选已取消。请重试。");
			pickBtn.Visible = true;
			Unlock();
			return;
		}
		status.ForeColor = Info;
		status.Text = "已识别（" + QrScanner.KindOf(hit.Text) + "），正在向 Okta 验证 / 换取种子…";
		Application.DoEvents();
		string err = SeedSaver.TrySave(app, hit.Text);
		if (err == null)
		{
			// 验证成功：弹回主界面，开始计算
			Close();
			app.ShowMain();
			return;
		}
		status.ForeColor = Bad;
		status.Text = err + "\n可重试，或点“手动粘贴”自己输入。";
		pickBtn.Visible = true;
		Unlock();
	}

	private void Unlock()
	{
		nextBtn.Enabled = true;
		pickBtn.Enabled = true;
		manualBtn.Enabled = true;
	}

	protected override void OnFormClosed(FormClosedEventArgs e)
	{
		SaveDomain(null, null);
		base.OnFormClosed(e);
	}
}
