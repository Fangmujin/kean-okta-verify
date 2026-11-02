using System.Drawing;
using System.Windows.Forms;

namespace KeanOkta;

public class HotkeyDialog : Form
{
	private Label info;

	private Button rec;

	private Button save;

	private KeyboardHook captureHook;

	private HotkeySpec candidate;

	public HotkeyDialog(AppContext app)
	{
		HotkeyDialog hotkeyDialog = this;
		Text = "自定义热键";
		base.Icon = AppContext.Icon;
		base.Width = 470;
		base.Height = 250;
		base.StartPosition = FormStartPosition.CenterParent;
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		Theme.ApplyDialog(this);
		info = new Label
		{
			Left = 16,
			Top = 18,
			Width = 422,
			Height = 120,
			Font = Theme.Ui(11f),
			ForeColor = Theme.Text
		};
		SetInfo("当前热键：" + app.Settings.Hotkey.Display + "\r\n\r\n点“开始录制”，然后按下你想要的组合键\r\n（支持左/右 Shift、Ctrl、Alt、Win）。", Theme.Text);
		rec = Theme.GoldButton("开始录制");
		rec.Left = 16;
		rec.Top = 165;
		rec.Width = 100;
		save = Theme.GoldButton("保存");
		save.Left = 238;
		save.Top = 165;
		save.Width = 90;
		save.Enabled = false;
		Button button = Theme.GoldButton("取消");
		button.Left = 348;
		button.Top = 165;
		button.Width = 90;
		button.DialogResult = DialogResult.Cancel;
		base.Controls.AddRange(new Control[4] { info, rec, save, button });
		base.AcceptButton = save;
		base.CancelButton = button;
		captureHook = new KeyboardHook(() => app.Settings.Hotkey);
		captureHook.Captured += OnCaptured;
		rec.Click += delegate
		{
			hotkeyDialog.candidate = null;
			hotkeyDialog.save.Enabled = false;
			hotkeyDialog.SetInfo("请按下组合键（按住修饰键再按主键）…", Theme.Blue);
			hotkeyDialog.captureHook.StartCapture();
		};
		save.Click += delegate
		{
			if (hotkeyDialog.candidate != null)
			{
				app.Settings.Hotkey = hotkeyDialog.candidate;
				Store.SaveSettings(app.Settings);
				hotkeyDialog.DialogResult = DialogResult.OK;
			}
		};
	}

	private void SetInfo(string text, Color c)
	{
		info.ForeColor = c;
		info.Text = text;
	}

	private void OnCaptured(HotkeySpec spec)
	{
		BeginInvoke((MethodInvoker)delegate
		{
			candidate = spec;
			if (HotkeyTester.CheckFree(spec, out var problem))
			{
				save.Enabled = true;
				SetInfo("候选热键：" + spec.Display + "\r\n\r\n未检测到冲突，点“保存”生效。", Theme.Green);
			}
			else
			{
				save.Enabled = false;
				SetInfo("候选热键：" + spec.Display + "\r\n\r\n不可用：" + problem + "\r\n请点“开始录制”更换。", Theme.Red);
			}
		});
	}

	protected override void OnFormClosed(FormClosedEventArgs e)
	{
		captureHook.Dispose();
		base.OnFormClosed(e);
	}
}
