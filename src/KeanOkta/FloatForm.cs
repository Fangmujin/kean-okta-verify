using System;
using System.Drawing;
using System.Windows.Forms;

namespace KeanOkta;

public class FloatForm : Form
{
	private AppContext app;

	private Label lab;

	private Timer t;

	private Point drag;

	private bool dragging;

	public FloatForm(AppContext app)
	{
		FloatForm floatForm = this;
		this.app = app;
		base.FormBorderStyle = FormBorderStyle.None;
		base.TopMost = true;
		base.ShowInTaskbar = false;
		BackColor = Theme.Bg;
		base.StartPosition = FormStartPosition.Manual;
		base.Size = new Size(app.Settings.FloatW, app.Settings.FloatH);
		if (app.Settings.FloatX >= 0)
		{
			base.Location = new Point(app.Settings.FloatX, app.Settings.FloatY);
		}
		else
		{
			Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
			base.Location = new Point(workingArea.Right - base.Width - 20, workingArea.Bottom - base.Height - 20);
		}
		lab = new Label
		{
			Dock = DockStyle.Fill,
			TextAlign = ContentAlignment.MiddleCenter,
			Font = Theme.MonoBold(Math.Max(16, base.Height / 3)),
			ForeColor = Theme.GoldBright,
			BackColor = Theme.Bg
		};
		base.Controls.Add(lab);
		ContextMenuStrip = new ContextMenuStrip
		{
			BackColor = Theme.BgPanel,
			ForeColor = Theme.Text,
			Font = Theme.Ui(9f),
			Items =
			{
				{
					"设置尺寸",
					(Image)null,
					(EventHandler)delegate
					{
						floatForm.EditSize();
					}
				},
				{
					"关闭悬浮窗",
					(Image)null,
					(EventHandler)delegate
					{
						floatForm.Close();
					}
				}
			}
		};
		base.MouseDown += delegate(object s, MouseEventArgs e)
		{
			floatForm.dragging = true;
			floatForm.drag = e.Location;
		};
		base.MouseMove += delegate(object s, MouseEventArgs e)
		{
			if (floatForm.dragging)
			{
				floatForm.Location = new Point(floatForm.Left + e.X - floatForm.drag.X, floatForm.Top + e.Y - floatForm.drag.Y);
			}
		};
		base.MouseUp += delegate
		{
			floatForm.dragging = false;
			floatForm.SaveGeo();
		};
		lab.MouseDown += delegate(object s, MouseEventArgs e)
		{
			floatForm.dragging = true;
			floatForm.drag = e.Location;
		};
		lab.MouseMove += delegate(object s, MouseEventArgs e)
		{
			if (floatForm.dragging)
			{
				floatForm.Location = new Point(floatForm.Left + e.X - floatForm.drag.X, floatForm.Top + e.Y - floatForm.drag.Y);
			}
		};
		lab.MouseUp += delegate
		{
			floatForm.dragging = false;
			floatForm.SaveGeo();
		};
		// 双击悬浮窗（含其上的验证码文字）→ 编辑尺寸大小
		base.MouseDoubleClick += delegate
		{
			floatForm.EditSize();
		};
		lab.MouseDoubleClick += delegate
		{
			floatForm.EditSize();
		};
		t = new Timer
		{
			Interval = 200
		};
		t.Tick += delegate
		{
			if (!string.IsNullOrEmpty(app.Seed))
			{
				byte[] key = Totp.B32Decode(app.Seed);
				floatForm.lab.Text = Totp.AtCounter(key, Totp.CounterAt(DateTimeOffset.UtcNow));
			}
		};
		t.Start();
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		// 黑金描边
		using (Pen pen = new Pen(Theme.GoldLine, 1f))
		{
			e.Graphics.DrawRectangle(pen, 0, 0, base.Width - 1, base.Height - 1);
		}
		using (Pen pen2 = new Pen(Theme.GoldFaint, 1f))
		{
			e.Graphics.DrawRectangle(pen2, 1, 1, base.Width - 3, base.Height - 3);
		}
	}

	// 编辑悬浮窗尺寸（双击文字 / 右键菜单均调用此方法）
	public void EditSize()
	{
		string text = InputDialog.Show(this, "设置悬浮窗尺寸", "宽 x 高（例如 168x72）：", base.Width + "x" + base.Height);
		if (text == null)
		{
			return;
		}
		try
		{
			string[] array = text.ToLower().Split('x');
			int num = Math.Max(80, int.Parse(array[0].Trim()));
			int num2 = Math.Max(36, int.Parse(array[1].Trim()));
			base.Size = new Size(num, num2);
			lab.Font = Theme.MonoBold(Math.Max(14, num2 / 3));
			SaveGeo();
		}
		catch
		{
			MessageBox.Show("格式应为 宽x高，例如 168x72");
		}
	}

	// 实时改尺寸：由“悬浮窗编辑”面板调用。宽高独立变化，验证码字号按高度等比缩放。
	// 缩放后尽量保持悬浮窗“中心点”不动，避免拖动跳位。
	public void ResizeTo(int w, int h)
	{
		w = Math.Max(80, w);
		h = Math.Max(36, h);
		int newLeft = base.Left + (base.Width - w) / 2;
		int newTop = base.Top + (base.Height - h) / 2;
		base.Size = new Size(w, h);
		base.Location = new Point(newLeft, newTop);
		lab.Font = Theme.MonoBold(Math.Max(14, h / 3));
		SaveGeo();
	}

	private void SaveGeo()
	{
		app.Settings.FloatW = base.Width;
		app.Settings.FloatH = base.Height;
		app.Settings.FloatX = base.Left;
		app.Settings.FloatY = base.Top;
		Store.SaveSettings(app.Settings);
	}

	protected override void OnFormClosed(FormClosedEventArgs e)
	{
		t.Stop();
		SaveGeo();
		base.OnFormClosed(e);
	}
}
