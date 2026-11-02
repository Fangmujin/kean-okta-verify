using System.Drawing;
using System.Windows.Forms;

namespace KeanOkta;

public static class InputDialog
{
	public static string Show(IWin32Window owner, string title, string label, string value)
	{
		Form form = new Form();
		form.Text = title;
		form.Width = 360;
		form.Height = 170;
		form.StartPosition = FormStartPosition.CenterParent;
		form.FormBorderStyle = FormBorderStyle.FixedDialog;
		form.MaximizeBox = false;
		form.MinimizeBox = false;
		Theme.ApplyDialog(form);
		using Form form2 = form;
		Label label2 = new Label
		{
			Left = 12,
			Top = 12,
			Width = 320,
			Text = label,
			ForeColor = Theme.Text,
			Font = Theme.Ui(9.5f)
		};
		Label label3 = label2;
		TextBox textBox = new TextBox
		{
			Left = 12,
			Top = 40,
			Width = 320,
			Text = value,
			BackColor = Theme.BgPanel,
			ForeColor = Theme.GoldBright,
			Font = Theme.Mono(10f),
			BorderStyle = BorderStyle.FixedSingle
		};
		TextBox textBox2 = textBox;
		Button button = Theme.GoldButton("确定");
		button.Left = 176;
		button.Top = 80;
		button.Width = 72;
		button.DialogResult = DialogResult.OK;
		Button button2 = button;
		button = Theme.GoldButton("取消");
		button.Left = 260;
		button.Top = 80;
		button.Width = 72;
		button.DialogResult = DialogResult.Cancel;
		Button button3 = button;
		form2.Controls.AddRange(new Control[4] { label3, textBox2, button2, button3 });
		form2.AcceptButton = button2;
		form2.CancelButton = button3;
		return (form2.ShowDialog(owner) != DialogResult.OK) ? null : textBox2.Text.Trim();
	}
}
