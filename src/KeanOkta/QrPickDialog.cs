using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace KeanOkta;

// 屏幕上识别到多个二维码时，让用户选一个（带缩略图预览）
public class QrPickDialog : Form
{
	private ListBox list;

	private PictureBox preview;

	private List<QrScanner.Hit> hits;

	private QrPickDialog(List<QrScanner.Hit> hits)
	{
		this.hits = hits;
		Text = "识别到多个二维码，请选择";
		base.Icon = AppContext.Icon;
		base.Width = 520;
		base.Height = 340;
		base.StartPosition = FormStartPosition.CenterParent;
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		Theme.ApplyDialog(this);
		Label label = new Label
		{
			Left = 14,
			Top = 12,
			Width = 488,
			Font = Theme.Ui(9.5f),
			ForeColor = Theme.TextDim,
			Text = "屏幕上找到多个二维码，请选择 Okta 设置页弹出的那一个（通常是 oktaverify:// 令牌）："
		};
		list = new ListBox
		{
			Left = 14,
			Top = 40,
			Width = 300,
			Height = 210,
			BackColor = Theme.BgPanel,
			ForeColor = Theme.Text,
			Font = Theme.Mono(9f),
			BorderStyle = BorderStyle.FixedSingle
		};
		list.SelectedIndexChanged += delegate
		{
			UpdatePreview();
		};
		preview = new PictureBox
		{
			Left = 326,
			Top = 40,
			Width = 176,
			Height = 176,
			BackColor = Theme.BgPanel,
			BorderStyle = BorderStyle.FixedSingle,
			SizeMode = PictureBoxSizeMode.Zoom
		};
		Label label2 = new Label
		{
			Left = 326,
			Top = 222,
			Width = 176,
			Height = 28,
			Font = Theme.Ui(8.5f),
			ForeColor = Theme.TextDim,
			Text = "预览图"
		};
		Button button = Theme.GoldButton("使用所选");
		button.Left = 330;
		button.Top = 262;
		button.Width = 90;
		button.DialogResult = DialogResult.OK;
		Button button2 = Theme.GoldButton("取消");
		button2.Left = 424;
		button2.Top = 262;
		button2.Width = 76;
		button2.DialogResult = DialogResult.Cancel;
		base.Controls.AddRange(new Control[6] { label, list, preview, label2, button, button2 });
		base.AcceptButton = button;
		base.CancelButton = button2;
		for (int i = 0; i < hits.Count; i++)
		{
			string item = (i + 1) + ". [" + QrScanner.KindOf(hits[i].Text) + "] " + QrScanner.Mask(hits[i].Text);
			list.Items.Add(item);
		}
		if (hits.Count > 0)
		{
			list.SelectedIndex = 0;
		}
	}

	private void UpdatePreview()
	{
		int selectedIndex = list.SelectedIndex;
		if (selectedIndex < 0 || selectedIndex >= hits.Count)
		{
			preview.Image = null;
			return;
		}
		Bitmap crop = hits[selectedIndex].Crop;
		if (preview.Image != null && preview.Image != crop)
		{
			preview.Image.Dispose();
		}
		preview.Image = ((crop != null) ? ((Image)(Image)crop.Clone()) : null);
	}

	// 返回用户选中的 Hit；取消返回 null
	public static QrScanner.Hit Pick(List<QrScanner.Hit> hits)
	{
		if (hits == null || hits.Count == 0)
		{
			return null;
		}
		if (hits.Count == 1)
		{
			return hits[0];
		}
		QrPickDialog qrPickDialog = new QrPickDialog(hits);
		if (qrPickDialog.ShowDialog() == DialogResult.OK && qrPickDialog.list.SelectedIndex >= 0)
		{
			return hits[qrPickDialog.list.SelectedIndex];
		}
		return null;
	}
}
