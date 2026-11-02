using System;
using System.Drawing;
using System.Windows.Forms;

namespace KeanOkta;

public class SeedDialog : Form
{
	private AppContext app;

	private TextBox box;

	private Label status;

	public SeedDialog(AppContext app)
	{
		this.app = app;
		Text = "配置种子";
		base.Icon = AppContext.Icon;
		base.Width = 560;
		base.Height = 360;
		base.StartPosition = FormStartPosition.CenterParent;
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		Theme.ApplyDialog(this);
		Label label = new Label
		{
			Left = 14,
			Top = 12,
			Width = 520,
			Text = "粘贴 oktaverify:// 令牌（程序自动联网换取种子），或直接粘贴 Base32 种子：",
			ForeColor = Theme.Text,
			Font = Theme.Ui(9.5f)
		};
		box = new TextBox
		{
			Left = 14,
			Top = 38,
			Width = 516,
			Height = 150,
			Multiline = true,
			ScrollBars = ScrollBars.Vertical,
			Font = Theme.Mono(10f),
			BackColor = Theme.BgPanel,
			ForeColor = Theme.GoldBright,
			BorderStyle = BorderStyle.FixedSingle
		};
		status = new Label
		{
			Left = 14,
			Top = 196,
			Width = 516,
			Height = 60,
			ForeColor = Theme.TextDim,
			Font = Theme.Ui(9.5f)
		};
		Button button = Theme.GoldButton("保存");
		button.Left = 370;
		button.Top = 270;
		button.Width = 72;
		button.DialogResult = DialogResult.None;
		Button button2 = Theme.GoldButton("取消");
		button2.Left = 458;
		button2.Top = 270;
		button2.Width = 72;
		button2.DialogResult = DialogResult.Cancel;
		button.Click += DoSave;
		base.Controls.AddRange(new Control[5] { label, box, status, button, button2 });
		base.AcceptButton = button;
		base.CancelButton = button2;
	}

	private void DoSave(object s, EventArgs e)
	{
		string text = box.Text.Trim();
		if (text.Length == 0)
		{
			status.ForeColor = Theme.Red;
			status.Text = "内容为空";
			return;
		}
		status.ForeColor = Theme.Blue;
		status.Text = ((text.StartsWith("oktaverify://") || text.StartsWith("otpauth://")) ? "正在向 Okta 换取种子…" : "正在保存…");
		Application.DoEvents();
		string err = SeedSaver.TrySave(app, text);
		if (err != null)
		{
			status.ForeColor = Theme.Red;
			status.Text = err;
			return;
		}
		status.ForeColor = Theme.Green;
		status.Text = "已保存种子，主界面将实时显示验证码。";
		base.DialogResult = DialogResult.OK;
	}
}
