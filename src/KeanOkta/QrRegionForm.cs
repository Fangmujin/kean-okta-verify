using System;
using System.Drawing;
using System.Windows.Forms;

namespace KeanOkta;

// 框选识别用的全屏遮罩：屏幕变暗，鼠标拖框圈住二维码，回车/松开确认，Esc 取消。
// 窗体Bounds = 虚拟屏，因此“窗体坐标”与“截屏坐标”一致（都以虚拟屏左上为原点）。
public class QrRegionForm : Form
{
	private Point start;

	private bool dragging;

	private Rectangle sel = Rectangle.Empty;

	public Rectangle Result = Rectangle.Empty;

	public QrRegionForm()
	{
		FormBorderStyle = FormBorderStyle.None;
		StartPosition = FormStartPosition.Manual;
		Bounds = SystemInformation.VirtualScreen;
		TopMost = true;
		BackColor = Color.Black;
		Opacity = 0.45;
		Cursor = Cursors.Cross;
		KeyPreview = true;
		base.Paint += OnPaint;
		base.MouseDown += delegate(object s, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				dragging = true;
				start = e.Location;
				sel = new Rectangle(e.Location, Size.Empty);
				Invalidate();
			}
		};
		base.MouseMove += delegate(object s, MouseEventArgs e)
		{
			if (dragging)
			{
				sel = new Rectangle(Math.Min(start.X, e.X), Math.Min(start.Y, e.Y), Math.Abs(e.X - start.X), Math.Abs(e.Y - start.Y));
				Invalidate();
			}
		};
		base.MouseUp += delegate(object s, MouseEventArgs e)
		{
			if (dragging && e.Button == MouseButtons.Left)
			{
				dragging = false;
				Result = sel;
				Close();
			}
		};
		base.KeyDown += delegate(object s, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Escape)
			{
				Result = Rectangle.Empty;
				Close();
			}
		};
	}

	private void OnPaint(object s, PaintEventArgs e)
	{
		if (dragging && sel.Width > 2 && sel.Height > 2)
		{
			using Pen pen = new Pen(Color.LimeGreen, 2f);
			e.Graphics.DrawRectangle(pen, sel.X, sel.Y, sel.Width - 1, sel.Height - 1);
			string text = sel.Width + " × " + sel.Height;
			Point location = new Point(sel.X, Math.Max(2, sel.Y - 24));
			TextRenderer.DrawText((IDeviceContext)e.Graphics, text, Theme.Mono(10f), location, Color.White);
		}
		else
		{
			string text2 = "拖框圈住二维码后松开（Esc 取消）";
			Size size = TextRenderer.MeasureText(text2, Theme.Ui(13f));
			Point location2 = new Point((base.Width - size.Width) / 2, 48);
			TextRenderer.DrawText((IDeviceContext)e.Graphics, text2, Theme.Ui(13f), location2, Color.White);
		}
	}

	public static Rectangle Pick()
	{
		QrRegionForm qrRegionForm = new QrRegionForm();
		qrRegionForm.ShowDialog();
		return qrRegionForm.Result;
	}
}
