using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace KeanOkta;

public class CodesPanel : UserControl
{
	private class AnimLabel
	{
		private Control host;

		private Label lab;

		private float x0;

		private float x1;

		private float size0;

		private float size1;

		private Color c0;

		private Color c1;

		public AnimLabel(Control host, string text, Color c0, float x0, float x1, float fs0, float fs1, Color cc0, Color cc1)
		{
			this.host = host;
			this.x0 = x0;
			this.x1 = x1;
			size0 = fs0;
			size1 = fs1;
			this.c0 = cc0;
			c1 = cc1;
			lab = new Label
			{
				Text = text,
				TextAlign = ContentAlignment.MiddleCenter,
				BackColor = host.BackColor,
				Font = Theme.MonoBold(Math.Max(8f, fs0))
			};
		}

		public void Make()
		{
			host.Controls.Add(lab);
		}

		public void Step(float p, int w, int h)
		{
			float num = x0 + (x1 - x0) * p;
			float val = size0 + (size1 - size0) * p;
			int num2 = (int)((float)w * 0.36f);
			int num3 = (int)(num * (float)w);
			lab.Font = Theme.MonoBold(Math.Max(8f, val));
			lab.SetBounds(num3 - num2 / 2, 0, num2, h);
			lab.ForeColor = Lerp(c0, c1, p);
		}

		private static Color Lerp(Color a, Color b, float p)
		{
			return Color.FromArgb((int)((float)(int)a.R + (float)(b.R - a.R) * p), (int)((float)(int)a.G + (float)(b.G - a.G) * p), (int)((float)(int)a.B + (float)(b.B - a.B) * p));
		}
	}

	// 黑金配色：当前码=亮金，上一/下一组=暗金，滑出时=极暗金（替代原“变黑”）
	private static readonly Color CurColor = Theme.GoldBright;

	private static readonly Color SideColor = Theme.GoldDim;

	private static readonly Color OutColor = Theme.GoldFaint;

	private TableLayoutPanel bigRow;

	private Label prevLab;

	private Label curLab;

	private Label nextLab;

	private Label prevSub;

	private Label curSub;

	private Label nextSub;

	private long lastCounter = -1L;

	private Panel overlay;

	private Timer anim;

	private int animFrame;

	private int animFrames = 28;

	public CodesPanel()
	{
		Dock = DockStyle.Fill;
		BackColor = Theme.Bg;
		TableLayoutPanel tableLayoutPanel = new TableLayoutPanel
		{
			Dock = DockStyle.Fill,
			RowCount = 2,
			ColumnCount = 1,
			BackColor = Theme.Bg
		};
		tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 76f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 24f));
		bigRow = new TableLayoutPanel
		{
			Dock = DockStyle.Fill,
			ColumnCount = 3,
			RowCount = 1,
			BackColor = Theme.Bg
		};
		for (int i = 0; i < 3; i++)
		{
			bigRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
		}
		bigRow.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
		prevLab = Static(SideColor);
		curLab = Static(CurColor);
		nextLab = Static(SideColor);
		bigRow.Controls.Add(prevLab, 0, 0);
		bigRow.Controls.Add(curLab, 1, 0);
		bigRow.Controls.Add(nextLab, 2, 0);
		tableLayoutPanel.Controls.Add(bigRow, 0, 0);
		TableLayoutPanel tableLayoutPanel2 = new TableLayoutPanel
		{
			Dock = DockStyle.Fill,
			ColumnCount = 3,
			RowCount = 1,
			BackColor = Theme.Bg
		};
		for (int j = 0; j < 3; j++)
		{
			tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
		}
		tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
		prevSub = Sub();
		curSub = Sub();
		nextSub = Sub();
		tableLayoutPanel2.Controls.Add(prevSub, 0, 0);
		tableLayoutPanel2.Controls.Add(curSub, 1, 0);
		tableLayoutPanel2.Controls.Add(nextSub, 2, 0);
		tableLayoutPanel.Controls.Add(tableLayoutPanel2, 0, 1);
		base.Controls.Add(tableLayoutPanel);
	}

	private Label Static(Color c)
	{
		Label label = new Label();
		label.Dock = DockStyle.Fill;
		label.TextAlign = ContentAlignment.MiddleCenter;
		label.ForeColor = c;
		label.Text = "------";
		label.Font = Theme.MonoBold(20f);
		return label;
	}

	private Label Sub()
	{
		Label label = new Label();
		label.Dock = DockStyle.Fill;
		label.TextAlign = ContentAlignment.MiddleCenter;
		label.ForeColor = Theme.TextDim;
		label.Font = Theme.Mono(10f);
		return label;
	}

	private float BigSize()
	{
		float val = (float)base.Height * 0.38f;
		float val2 = (float)base.Width / 3f / 5f;
		return Math.Max(16f, Math.Min(val, val2));
	}

	public void Refresh(string seed, DateTimeOffset now, double unixFloat)
	{
		if (string.IsNullOrEmpty(seed))
		{
			prevLab.Text = "------";
			nextLab.Text = "------";
			curLab.Text = "------";
			prevSub.Text = "";
			nextSub.Text = "";
			curSub.Text = "";
			return;
		}
		byte[] key = Totp.B32Decode(seed);
		long num = Totp.CounterAt(now);
		string text2 = Totp.AtCounter(key, num - 1);
		string text3 = Totp.AtCounter(key, num);
		string text4 = Totp.AtCounter(key, num + 1);
		double num2 = (double)num * 30.0;
		double num3 = (double)(num + 1) * 30.0;
		double num4 = unixFloat - num2;
		double num5 = num3 - unixFloat;
		prevSub.Text = "-" + num4.ToString("F3") + "s";
		curSub.Text = num5.ToString("F3") + "s";
		nextSub.Text = "+" + num5.ToString("F3") + "s";
		if (num != lastCounter)
		{
			if (lastCounter == -1 || anim != null)
			{
				SetStatic(text2, text3, text4, prevBlack: false);
			}
			else
			{
				StartAnimation(text2, text3, text4);
			}
			lastCounter = num;
		}
		else if (anim == null)
		{
			float num6 = BigSize();
			if (Math.Abs(curLab.Font.Size - num6) > 0.6f)
			{
				ApplyFonts();
			}
		}
	}

	private void ApplyFonts()
	{
		float num = BigSize();
		float emSize = num * 0.42f;
		curLab.Font = Theme.MonoBold(num);
		prevLab.Font = Theme.MonoBold(emSize);
		nextLab.Font = Theme.MonoBold(emSize);
	}

	private void SetStatic(string prev, string cur, string next, bool prevBlack)
	{
		prevLab.Text = prev;
		curLab.Text = cur;
		nextLab.Text = next;
		prevLab.ForeColor = ((!prevBlack) ? SideColor : OutColor);
		curLab.ForeColor = CurColor;
		nextLab.ForeColor = SideColor;
		ApplyFonts();
	}

	protected override void OnResize(EventArgs e)
	{
		base.OnResize(e);
		if (anim == null)
		{
			ApplyFonts();
		}
	}

	private void StartAnimation(string newPrev, string newCur, string newNext)
	{
		string text = prevLab.Text;
		string oldCur = curLab.Text;
		string oldNext = nextLab.Text;
		try
		{
			bigRow.Visible = false;
			overlay = new Panel
			{
				BackColor = Theme.Bg
			};
			Rectangle bounds = bigRow.Bounds;
			overlay.Bounds = new Rectangle(bounds.X, bounds.Y, Math.Max(1, bounds.Width), Math.Max(1, bounds.Height));
			base.Controls.Add(overlay);
			overlay.BringToFront();
			float num = BigSize();
			float num2 = num * 0.42f;
			List<AnimLabel> L = new List<AnimLabel>
			{
				new AnimLabel(overlay, text, OutColor, 0f, -1f / 3f, num2, num2, OutColor, OutColor),
				new AnimLabel(overlay, oldCur, CurColor, 0.5f, 1f / 6f, num, num2, CurColor, OutColor),
				new AnimLabel(overlay, oldNext, SideColor, 5f / 6f, 0.5f, num2, num, SideColor, CurColor),
				new AnimLabel(overlay, newNext, SideColor, 1.1666666f, 5f / 6f, num2, num2, SideColor, SideColor)
			};
			foreach (AnimLabel item in L)
			{
				item.Make();
			}
			animFrame = 0;
			anim = new Timer
			{
				Interval = 16
			};
			anim.Tick += delegate
			{
				try
				{
					animFrame++;
					float num3 = Math.Min(1f, (float)animFrame / (float)animFrames);
					float p = 1f - (1f - num3) * (1f - num3);
					foreach (AnimLabel item2 in L)
					{
						item2.Step(p, overlay.Width, overlay.Height);
					}
					if (num3 >= 1f)
					{
						FinishAnim(oldCur, oldNext, newNext);
					}
				}
				catch (Exception ex2)
				{
					Console.WriteLine("anim tick: " + ex2.Message);
					FinishAnim(oldCur, oldNext, newNext);
				}
			};
			anim.Start();
		}
		catch (Exception ex)
		{
			Console.WriteLine("anim start: " + ex.Message);
			FinishAnim(oldCur, oldNext, newNext);
		}
	}

	private void FinishAnim(string oldCur, string oldNext, string newNext)
	{
		try
		{
			if (anim != null)
			{
				anim.Stop();
				anim.Dispose();
				anim = null;
			}
			if (overlay != null)
			{
				base.Controls.Remove(overlay);
				overlay.Dispose();
				overlay = null;
			}
			bigRow.Visible = true;
			SetStatic(oldCur, oldNext, newNext, prevBlack: true);
		}
		catch
		{
		}
	}
}
