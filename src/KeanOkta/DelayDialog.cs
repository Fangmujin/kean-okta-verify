using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace KeanOkta;

public class DelayDialog : Form
{
	private AppContext app;

	private Label desc;

	private Label progress;

	private Button startBtn;

	private Button cancelBtn;

	private DelayRecorder recorder;

	private int cachedCount;

	public DelayDialog(AppContext app)
	{
		this.app = app;
		Text = "延迟模拟 · 录制按键节奏";
		base.Icon = AppContext.Icon;
		base.Width = 460;
		base.Height = 300;
		base.StartPosition = FormStartPosition.CenterParent;
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		Theme.ApplyDialog(this);

		desc = new Label
		{
			Left = 16,
			Top = 14,
			Width = 428,
			Height = 96,
			Font = Theme.Ui(10f),
			ForeColor = Theme.Text,
			Text = "为了更逼真地绕过“脚本防注入”检测，程序会录制你的真实按键节奏。\r\n\r\n点“开始录制”后，正常地连续按下 6 个数字键（主键盘 0-9 或小键盘均可），\r\n程序会记录你前 5 次按键之间的间隔；之后热键录入验证码时，\r\n会从这 5 个间隔里随机取一个，再叠加 1~15ms 随机抖动，模拟真人。"
		};

		progress = new Label
		{
			Left = 16,
			Top = 124,
			Width = 428,
			Height = 70,
			Font = Theme.MonoBold(12f),
			ForeColor = Theme.GoldBright,
			Text = "当前状态：" + DescribeSamples(app.Settings.DelaySamples)
		};

		startBtn = Theme.GoldButton("开始录制");
		startBtn.Left = 16;
		startBtn.Top = 214;
		startBtn.Width = 120;

		cancelBtn = Theme.GoldButton("关闭");
		cancelBtn.Left = 324;
		cancelBtn.Top = 214;
		cancelBtn.Width = 90;
		cancelBtn.DialogResult = DialogResult.Cancel;

		base.Controls.AddRange(new Control[4] { desc, progress, startBtn, cancelBtn });
		base.CancelButton = cancelBtn;

		startBtn.Click += OnStart;
	}

	private static string DescribeSamples(int[] samples)
	{
		if (samples == null || samples.Length == 0)
		{
			return "尚未录制（热键输入将使用默认 45ms + 1~15ms 抖动）";
		}
		return "已录制 " + samples.Length + " 个间隔：" + JoinSamples(samples) + " ms";
	}

	private static string JoinSamples(int[] samples)
	{
		System.Text.StringBuilder sb = new System.Text.StringBuilder();
		for (int i = 0; i < samples.Length; i++)
		{
			if (i > 0)
			{
				sb.Append("  ");
			}
			sb.Append(samples[i]);
		}
		return sb.ToString();
	}

	private void OnStart(object s, EventArgs e)
	{
		if (recorder != null)
		{
			recorder.Dispose();
			recorder = null;
		}
		cachedCount = 0;
		recorder = new DelayRecorder();
		recorder.TargetCount = 5;
		recorder.Pressed += delegate(int count)
		{
			cachedCount = count;
			BeginInvoke((MethodInvoker)delegate
			{
				progress.ForeColor = Theme.Blue;
				progress.Text = "已录 " + Math.Min(5, cachedCount) + " / 5 个间隔，请继续按数字键…";
			});
		};
		recorder.Completed += delegate(int[] result)
		{
			BeginInvoke((MethodInvoker)delegate
			{
				app.Settings.DelaySamples = result;
				Store.SaveSettings(app.Settings);
				progress.ForeColor = Theme.Green;
				progress.Text = "录制完成，已保存。\r\n间隔：" + JoinSamples(result) + " ms\r\n热键录入现在会按你的节奏 + 随机抖动输入。";
				startBtn.Text = "重新录制";
				if (recorder != null)
				{
					recorder.Dispose();
					recorder = null;
				}
			});
		};
		progress.ForeColor = Theme.Blue;
		progress.Text = "正在监听…请连续按 6 个数字键（0-9）";
		startBtn.Text = "录制中…";
	}

	protected override void OnFormClosed(FormClosedEventArgs e)
	{
		if (recorder != null)
		{
			recorder.Dispose();
			recorder = null;
		}
		base.OnFormClosed(e);
	}
}
