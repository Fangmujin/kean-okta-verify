using System;
using System.Drawing;
using System.Windows.Forms;

namespace KeanOkta;

// 黑金 · 数学/计算机高级风格主题。集中定义配色、字体与控件样式，供各窗体复用。
public static class Theme
{
	// —— 背景 ——
	public static readonly Color Bg = Color.FromArgb(13, 13, 15);          // 主背景 深黑
	public static readonly Color BgPanel = Color.FromArgb(22, 22, 27);     // 面板 炭黑
	public static readonly Color BgPanel2 = Color.FromArgb(28, 28, 34);    // 次级面板
	public static readonly Color BgFormula = Color.FromArgb(16, 16, 20);   // 公式区 近黑

	// —— 金色系 ——
	public static readonly Color Gold = Color.FromArgb(212, 175, 55);       // 经典金
	public static readonly Color GoldBright = Color.FromArgb(245, 208, 97); // 亮金（当前码/高亮）
	public static readonly Color GoldDim = Color.FromArgb(150, 122, 58);    // 暗金（上一组/次级）
	public static readonly Color GoldFaint = Color.FromArgb(96, 80, 44);    // 极暗金（边框/装饰）
	public static readonly Color GoldLine = Color.FromArgb(60, 52, 28);     // 描边金

	// —— 文字 ——
	public static readonly Color Text = Color.FromArgb(232, 230, 224);      // 主文字 暖白
	public static readonly Color TextDim = Color.FromArgb(154, 151, 142);   // 次级文字
	public static readonly Color TextFaint = Color.FromArgb(110, 108, 100); // 弱文字
	public static readonly Color Green = Color.FromArgb(120, 200, 130);     // 成功
	public static readonly Color Red = Color.FromArgb(224, 96, 96);         // 错误
	public static readonly Color Blue = Color.FromArgb(120, 170, 230);      // 信息

	// —— 字体 ——
	public static Font Mono(float size, FontStyle style = FontStyle.Regular)
	{
		return new Font("Consolas", size, style);
	}

	public static Font MonoBold(float size)
	{
		return new Font("Consolas", size, FontStyle.Bold);
	}

	public static Font Ui(float size, FontStyle style = FontStyle.Regular)
	{
		return new Font("Segoe UI", size, style);
	}

	public static Font UiBold(float size)
	{
		return new Font("Segoe UI", size, FontStyle.Bold);
	}

	// 创建黑底金边按钮
	public static Button GoldButton(string text, float fontSize = 10f)
	{
		Button b = new Button
		{
			Text = text,
			FlatStyle = FlatStyle.Flat,
			BackColor = BgPanel2,
			ForeColor = GoldBright,
			Font = Ui(fontSize),
			TabStop = true,
			Cursor = Cursors.Hand
		};
		b.FlatAppearance.BorderColor = GoldLine;
		b.FlatAppearance.BorderSize = 1;
		b.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, 36, 24);
		b.FlatAppearance.MouseDownBackColor = Color.FromArgb(52, 46, 28);
		return b;
	}

	// 创建黑底金边的“按钮样式”复选框（用于开关类按钮）
	public static CheckBox GoldToggle(string text, float fontSize = 10f)
	{
		CheckBox c = new CheckBox
		{
			Text = text,
			Appearance = Appearance.Button,
			FlatStyle = FlatStyle.Flat,
			BackColor = BgPanel2,
			ForeColor = GoldBright,
			Font = Ui(fontSize),
			TextAlign = ContentAlignment.MiddleCenter,
			CheckAlign = ContentAlignment.MiddleCenter,
			Cursor = Cursors.Hand
		};
		c.FlatAppearance.BorderColor = GoldLine;
		c.FlatAppearance.BorderSize = 1;
		c.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, 36, 24);
		c.FlatAppearance.MouseDownBackColor = Color.FromArgb(52, 46, 28);
		return c;
	}

	// 给窗体套黑金底
	public static void ApplyForm(Form f)
	{
		f.BackColor = Bg;
		f.ForeColor = Text;
	}

	// 给对话框套黑金底
	public static void ApplyDialog(Form f)
	{
		f.BackColor = Bg;
		f.ForeColor = Text;
	}

	// 托盘/右键菜单的黑金渲染器：黑底、金边、暖白文字、金色高亮
	public class BlackGoldRenderer : ToolStripProfessionalRenderer
	{
		public BlackGoldRenderer()
			: base(new BlackGoldColors())
		{
		}

		protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
		{
			e.TextColor = e.Item.Enabled ? Text : TextFaint;
			base.OnRenderItemText(e);
		}

		protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
		{
			e.ArrowColor = Gold;
			base.OnRenderArrow(e);
		}

		protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
		{
			Rectangle r = e.Item.Bounds;
			using (Pen pen = new Pen(GoldLine, 1f))
			{
				e.Graphics.DrawLine(pen, r.Left + 2, r.Top + r.Height / 2, r.Right - 2, r.Top + r.Height / 2);
			}
		}
	}

	// 仅重写 .NET 4.8 中确实存在的 ProfessionalColorTable 属性
	public class BlackGoldColors : ProfessionalColorTable
	{
		public override Color MenuStripGradientBegin => BgPanel;
		public override Color MenuStripGradientEnd => BgPanel;
		public override Color ToolStripDropDownBackground => BgPanel;
		public override Color ImageMarginGradientBegin => BgPanel;
		public override Color ImageMarginGradientMiddle => BgPanel;
		public override Color ImageMarginGradientEnd => BgPanel;
		public override Color MenuItemSelected => Color.FromArgb(45, 40, 26);
		public override Color MenuItemSelectedGradientBegin => Color.FromArgb(45, 40, 26);
		public override Color MenuItemSelectedGradientEnd => Color.FromArgb(52, 46, 30);
		public override Color MenuItemBorder => GoldLine;
		public override Color MenuBorder => GoldLine;
		public override Color SeparatorDark => GoldLine;
		public override Color SeparatorLight => GoldLine;
		public override Color ButtonSelectedHighlight => Color.FromArgb(45, 40, 26);
		public override Color ButtonSelectedHighlightBorder => GoldLine;
		public override Color ButtonSelectedGradientBegin => Color.FromArgb(45, 40, 26);
		public override Color ButtonSelectedGradientEnd => Color.FromArgb(52, 46, 30);
		public override Color ButtonSelectedBorder => GoldLine;
		public override Color ButtonPressedGradientBegin => Color.FromArgb(52, 46, 30);
		public override Color ButtonPressedGradientEnd => Color.FromArgb(52, 46, 30);
		public override Color ButtonPressedBorder => GoldLine;
		public override Color ButtonCheckedGradientBegin => Color.FromArgb(45, 40, 26);
		public override Color ButtonCheckedGradientEnd => Color.FromArgb(52, 46, 30);
		public override Color ButtonCheckedHighlight => Color.FromArgb(45, 40, 26);
		public override Color GripDark => GoldLine;
		public override Color GripLight => GoldLine;
		public override Color OverflowButtonGradientBegin => BgPanel;
		public override Color OverflowButtonGradientEnd => BgPanel;
		public override Color ToolStripBorder => GoldLine;
		public override Color ToolStripContentPanelGradientBegin => BgPanel;
		public override Color ToolStripContentPanelGradientEnd => BgPanel;
		public override Color ToolStripGradientBegin => BgPanel;
		public override Color ToolStripGradientMiddle => BgPanel;
		public override Color ToolStripGradientEnd => BgPanel;
		public override Color ToolStripPanelGradientBegin => BgPanel;
		public override Color ToolStripPanelGradientEnd => BgPanel;
		public override Color CheckBackground => BgPanel;
		public override Color CheckSelectedBackground => Color.FromArgb(45, 40, 26);
	}
}
