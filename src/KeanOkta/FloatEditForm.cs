using System;
using System.Drawing;
using System.Windows.Forms;

namespace KeanOkta;

// “悬浮窗编辑”面板：滑块 + 数字框同步调整悬浮窗宽 / 高（像素），
// 改动实时反映到悬浮窗（数字随高度等比例放大缩小）。
public class FloatEditForm : Form
{
	private AppContext app;

	private TrackBar barW;

	private TrackBar barH;

	private NumericUpDown numW;

	private NumericUpDown numH;

	private Label status;

	private bool applying;

	public FloatEditForm(AppContext app)
	{
		this.app = app;
		Text = "悬浮窗编辑 · 实时调整尺寸";
		base.Icon = AppContext.Icon;
		base.Width = 440;
		base.Height = 300;
		base.StartPosition = FormStartPosition.CenterParent;
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		Theme.ApplyDialog(this);

		Label titleW = MakeLabel("宽度（像素）", 16, 16, 200);
		numW = MakeNumeric(200, 16, 16, 80, app.Settings.FloatW);
		barW = MakeBar(48, 80, app.Settings.FloatW);

		Label titleH = MakeLabel("高度（像素）", 16, 120, 200);
		numH = MakeNumeric(200, 120, 16, 36, app.Settings.FloatH);
		barH = MakeBar(152, 36, app.Settings.FloatH);

		status = new Label
		{
			Left = 16,
			Top = 196,
			Width = 408,
			Height = 56,
			Font = Theme.Ui(9.5f),
			ForeColor = Theme.TextDim,
			Text = "拖动滑块或修改数字，右侧悬浮窗会实时变化。\r\n验证码字号随高度等比缩放；关闭面板后尺寸自动保存。"
		};

		Button closeBtn = Theme.GoldButton("完成");
		closeBtn.Left = 334;
		closeBtn.Top = 252;
		closeBtn.Width = 90;
		closeBtn.DialogResult = DialogResult.OK;

		base.Controls.AddRange(new Control[8] { titleW, numW, barW, titleH, numH, barH, status, closeBtn });
		base.AcceptButton = closeBtn;
		base.CancelButton = closeBtn;

		// 滑块 ↔ 数字框 双向同步，并实时作用到悬浮窗
		barW.Scroll += delegate { SyncW(barW.Value); };
		barH.Scroll += delegate { SyncH(barH.Value); };
		numW.ValueChanged += delegate { SyncW((int)numW.Value); };
		numH.ValueChanged += delegate { SyncH((int)numH.Value); };
	}

	private static Label MakeLabel(string text, int x, int y, int width)
	{
		return new Label
		{
			Left = x,
			Top = y,
			Width = width,
			Height = 20,
			Font = Theme.Ui(10f),
			ForeColor = Theme.Text,
			Text = text
		};
	}

	private static NumericUpDown MakeNumeric(int width, int x, int y, int min, int value)
	{
		NumericUpDown n = new NumericUpDown
		{
			Left = x,
			Top = y + 24,
			Width = width,
			Minimum = min,
			Maximum = 1200,
			Value = Math.Min(1200, Math.Max(min, value)),
			BackColor = Theme.BgPanel2,
			ForeColor = Theme.GoldBright,
			Font = Theme.Mono(10f),
			BorderStyle = BorderStyle.FixedSingle
		};
		return n;
	}

	private static TrackBar MakeBar(int y, int min, int value)
	{
		TrackBar t = new TrackBar
		{
			Left = 240,
			Top = y - 8,
			Width = 190,
			Minimum = min,
			Maximum = 1200,
			TickStyle = TickStyle.None,
			Value = Math.Min(1200, Math.Max(min, value)),
			BackColor = Theme.Bg
		};
		return t;
	}

	private void SyncW(int w)
	{
		if (applying)
		{
			return;
		}
		applying = true;
		numW.Value = Math.Min(1200, Math.Max(80, w));
		barW.Value = (int)numW.Value;
		app.ResizeFloat((int)numW.Value, (int)numH.Value);
		applying = false;
	}

	private void SyncH(int h)
	{
		if (applying)
		{
			return;
		}
		applying = true;
		numH.Value = Math.Min(1200, Math.Max(36, h));
		barH.Value = (int)numH.Value;
		app.ResizeFloat((int)numW.Value, (int)numH.Value);
		applying = false;
	}

	protected override void OnFormClosed(FormClosedEventArgs e)
	{
		// 确保最终尺寸落盘
		app.ResizeFloat((int)numW.Value, (int)numH.Value);
		base.OnFormClosed(e);
	}
}
