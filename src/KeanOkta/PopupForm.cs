using System;
using System.Drawing;
using System.Windows.Forms;

namespace KeanOkta;

public class PopupForm : Form
{
	private Label lab;

	private Timer t;

	public PopupForm(string seed)
	{
		PopupForm popupForm = this;
		base.FormBorderStyle = FormBorderStyle.None;
		base.StartPosition = FormStartPosition.Manual;
		base.ShowInTaskbar = false;
		BackColor = Theme.Bg;
		Rectangle workingArea = Screen.FromPoint(Cursor.Position).WorkingArea;
		int num = 190;
		int num2 = 96;
		base.Location = new Point(Math.Min(Cursor.Position.X, workingArea.Right - num - 8), Math.Min(Cursor.Position.Y + 14, workingArea.Bottom - num2 - 8));
		base.Size = new Size(num, num2);
		lab = new Label
		{
			Dock = DockStyle.Fill,
			TextAlign = ContentAlignment.MiddleCenter,
			Font = Theme.MonoBold(30f),
			ForeColor = Theme.GoldBright,
			BackColor = Theme.Bg
		};
		base.Controls.Add(lab);
		t = new Timer
		{
			Interval = 200
		};
		t.Tick += delegate
		{
			byte[] key = Totp.B32Decode(seed);
			popupForm.lab.Text = Totp.AtCounter(key, Totp.CounterAt(DateTimeOffset.UtcNow));
		};
		t.Start();
		base.Deactivate += delegate
		{
			popupForm.BeginInvoke(new MethodInvoker(popupForm.Close));
		};
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		using (Pen pen = new Pen(Theme.GoldLine, 1f))
		{
			e.Graphics.DrawRectangle(pen, 0, 0, base.Width - 1, base.Height - 1);
		}
	}

	protected override void OnFormClosed(FormClosedEventArgs e)
	{
		t.Stop();
		base.OnFormClosed(e);
	}
}
