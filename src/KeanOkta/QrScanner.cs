using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ZXing;
using ZXing.Common;

namespace KeanOkta;

// 屏幕二维码识别：截取整个虚拟屏（含多显示器）→ ZXing 解码。
// 策略：先快速解码；失败再 TryHarder；仍失败则 2 倍放大后 TryHarder（应对小二维码）。
// 注意：本进程为 DPI-unaware（保持 UI 缩放与旧版一致），SystemInformation.VirtualScreen
// 返回的是“虚拟化坐标”（如 150% 缩放的 2560x1600 屏只报 1707x1067），而 CopyFromScreen
// 按物理像素捕获。因此这里用屏幕物理 DPI 换算倍率，截取完整的物理屏幕。
public static class QrScanner
{
	public class Hit
	{
		public string Text;

		public Rectangle Bounds;

		public Bitmap Crop;
	}

	private const int ENUM_CURRENT_SETTINGS = -1;

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	private struct DEVMODE
	{
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
		public string dmDeviceName;

		public short dmSpecVersion;

		public short dmDriverVersion;

		public short dmSize;

		public short dmDriverExtra;

		public int dmFields;

		public int dmPositionX;

		public int dmPositionY;

		public int dmDisplayOrientation;

		public int dmDisplayFixedOutput;

		public short dmColor;

		public short dmDuplex;

		public short dmYResolution;

		public short dmTTOption;

		public short dmCollate;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
		public string dmFormName;

		public short dmLogPixels;

		public int dmBitsPerPel;

		public int dmPelsWidth;

		public int dmPelsHeight;

		public int dmDisplayFlags;

		public int dmDisplayFrequency;

		public int dmICMMethod;

		public int dmICMIntent;

		public int dmMediaType;

		public int dmDitherType;

		public int dmReserved1;

		public int dmReserved2;

		public int dmPanningWidth;

		public int dmPanningHeight;
	}

	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	private static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);

	// 屏幕物理 DPI / 96，即系统的显示缩放倍率（100% → 1.0，150% → 1.5）。
	// 注意：DPI-unaware 进程里 GetDC(0) 的设备 Caps 也被虚拟化（恒返回 96），不可用；
	// 必须用 EnumDisplaySettings 取主显示器物理分辨率，再与虚拟屏宽度求比值。
	public static double ScreenScale()
	{
		try
		{
			DEVMODE devMode = default(DEVMODE);
			devMode.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));
			if (!EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref devMode) || devMode.dmPelsWidth <= 0)
			{
				return 1.0;
			}
			Rectangle virtualScreen = SystemInformation.VirtualScreen;
			if (virtualScreen.Width <= 0)
			{
				return 1.0;
			}
			double num = (double)devMode.dmPelsWidth / (double)virtualScreen.Width;
			if (num < 0.5 || num > 4.0)
			{
				return 1.0;
			}
			return num;
		}
		catch
		{
			return 1.0;
		}
	}

	// 截取整个物理屏幕（多显示器拼接，左上角为物理坐标原点）。
	// 返回的位图为物理像素；位图坐标 = 虚拟屏坐标 × ScreenScale()。
	public static Bitmap CaptureScreen()
	{
		Rectangle virtualScreen = SystemInformation.VirtualScreen;
		double num = ScreenScale();
		int num2 = Math.Max(1, (int)Math.Round((double)virtualScreen.Width * num));
		int num3 = Math.Max(1, (int)Math.Round((double)virtualScreen.Height * num));
		int num4 = (int)Math.Round((double)virtualScreen.Left * num);
		int num5 = (int)Math.Round((double)virtualScreen.Top * num);
		Bitmap bitmap = new Bitmap(num2, num3, PixelFormat.Format32bppArgb);
		using (Graphics g = Graphics.FromImage(bitmap))
		{
			g.CopyFromScreen(num4, num5, 0, 0, new Size(num2, num3), CopyPixelOperation.SourceCopy);
		}
		return bitmap;
	}

	private static List<Hit> DecodeAll(Bitmap bmp, bool harder)
	{
		List<Hit> hits = new List<Hit>();
		if (bmp == null)
		{
			return hits;
		}
		try
		{
			BarcodeReader reader = new BarcodeReader
			{
				AutoRotate = true,
				TryInverted = true,
				Options = new DecodingOptions
				{
					TryHarder = harder,
					PossibleFormats = new List<BarcodeFormat> { BarcodeFormat.QR_CODE }
				}
			};
			Result[] results = reader.DecodeMultiple(bmp);
			if (results == null)
			{
				return hits;
			}
			foreach (Result r in results)
			{
				if (r == null || string.IsNullOrEmpty(r.Text))
				{
					continue;
				}
				Rectangle b = BoundsOf(r);
				bool dup = false;
				foreach (Hit h in hits)
				{
					if (h.Text == r.Text)
					{
						dup = true;
						break;
					}
				}
				if (dup)
				{
					continue;
				}
				hits.Add(new Hit
				{
					Text = r.Text,
					Bounds = b,
					Crop = CropOf(bmp, b)
				});
			}
		}
		catch
		{
		}
		return hits;
	}

	private static Rectangle BoundsOf(Result r)
	{
		if (r.ResultPoints == null || r.ResultPoints.Length == 0)
		{
			return Rectangle.Empty;
		}
		int num = int.MaxValue;
		int num2 = int.MaxValue;
		int num3 = 0;
		int num4 = 0;
		foreach (ResultPoint resultPoint in r.ResultPoints)
		{
			int num5 = (int)resultPoint.X;
			int num6 = (int)resultPoint.Y;
			num = Math.Min(num, num5);
			num2 = Math.Min(num2, num6);
			num3 = Math.Max(num3, num5);
			num4 = Math.Max(num4, num6);
		}
		if (num > num3 || num2 > num4)
		{
			return Rectangle.Empty;
		}
		return Rectangle.FromLTRB(num, num2, num3, num4);
	}

	private static Bitmap CropOf(Bitmap bmp, Rectangle b)
	{
		if (bmp == null || b.IsEmpty)
		{
			return null;
		}
		int num = 16;
		Rectangle r = Rectangle.FromLTRB(b.Left - num, b.Top - num, b.Right + num, b.Bottom + num);
		r.Intersect(new Rectangle(0, 0, bmp.Width, bmp.Height));
		if (r.Width <= 0 || r.Height <= 0)
		{
			return null;
		}
		try
		{
			return bmp.Clone(r, bmp.PixelFormat);
		}
		catch
		{
			return null;
		}
	}

	// 全屏识别：返回屏幕上所有二维码候选（可能多个）
	public static List<Hit> ScanScreen()
	{
		using (Bitmap bmp = CaptureScreen())
		{
			List<Hit> list = DecodeAll(bmp, harder: false);
			if (list.Count == 0)
			{
				list = DecodeAll(bmp, harder: true);
			}
			if (list.Count == 0)
			{
				// 小二维码兜底：适度放大（大屏只放 1.5 倍，避免超大临时位图）
				double num = ((bmp.Width * bmp.Height <= 4000000) ? 2.0 : 1.5);
				int num2 = (int)Math.Round((double)bmp.Width * num);
				int num3 = (int)Math.Round((double)bmp.Height * num);
				using (Bitmap big = new Bitmap(num2, num3, PixelFormat.Format32bppArgb))
				{
					using (Graphics g = Graphics.FromImage(big))
					{
						g.InterpolationMode = InterpolationMode.HighQualityBicubic;
						g.DrawImage(bmp, 0, 0, num2, num3);
					}
					list = DecodeAll(big, harder: true);
					foreach (Hit item in list)
					{
						if (!item.Bounds.IsEmpty)
						{
							item.Bounds = new Rectangle((int)Math.Round((double)item.Bounds.X / num), (int)Math.Round((double)item.Bounds.Y / num), Math.Max(1, (int)Math.Round((double)item.Bounds.Width / num)), Math.Max(1, (int)Math.Round((double)item.Bounds.Height / num)));
						}
					}
				}
			}
			return list;
		}
	}

	// 框选识别：全屏半透明遮罩，用户拖框圈住二维码后解码；取消返回 null。
	// 遮罩窗体坐标 = 虚拟屏坐标（窗体 Bounds = VirtualScreen），需换算成物理像素后再从截屏裁剪。
	public static Hit SelectAndScan()
	{
		Rectangle rect = QrRegionForm.Pick();
		if (rect.IsEmpty || rect.Width < 8 || rect.Height < 8)
		{
			return null;
		}
		double num = ScreenScale();
		Rectangle virtualScreen = SystemInformation.VirtualScreen;
		Rectangle r = new Rectangle((int)Math.Round((double)(rect.X + virtualScreen.Left) * num), (int)Math.Round((double)(rect.Y + virtualScreen.Top) * num), (int)Math.Round((double)rect.Width * num), (int)Math.Round((double)rect.Height * num));
		using (Bitmap bmp = CaptureScreen())
		{
			r.Intersect(new Rectangle(0, 0, bmp.Width, bmp.Height));
			if (r.Width < 8 || r.Height < 8)
			{
				return null;
			}
			using (Bitmap crop = bmp.Clone(r, bmp.PixelFormat))
			{
				List<Hit> list = DecodeAll(crop, harder: false);
				if (list.Count == 0)
				{
					list = DecodeAll(crop, harder: true);
				}
				Hit hit = PickBest(list);
				if (hit == null)
				{
					return null;
				}
				foreach (Hit item in list)
				{
					if (item != hit && item.Crop != null)
					{
						item.Crop.Dispose();
					}
				}
				return new Hit
				{
					Text = hit.Text,
					Bounds = r,
					Crop = (Bitmap)crop.Clone()
				};
			}
		}
	}

	// 从候选里挑最可能是 Okta 种子的：oktaverify:// 令牌优先，其次带 secret 的 otpauth://
	public static Hit PickBest(List<Hit> hits)
	{
		if (hits == null || hits.Count == 0)
		{
			return null;
		}
		foreach (Hit hit in hits)
		{
			if (hit.Text.StartsWith("oktaverify://"))
			{
				return hit;
			}
		}
		foreach (Hit hit2 in hits)
		{
			if (hit2.Text.StartsWith("otpauth://") && hit2.Text.Contains("secret="))
			{
				return hit2;
			}
		}
		return hits[0];
	}

	// 给界面显示用：把过长的二维码内容打码
	public static string Mask(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return "";
		}
		string text2 = text.Replace("\r", " ").Replace("\n", " ");
		if (text2.Length <= 42)
		{
			return text2;
		}
		return text2.Substring(0, 20) + " … " + text2.Substring(text2.Length - 18);
	}

	public static string KindOf(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return "空";
		}
		if (text.StartsWith("oktaverify://"))
		{
			return "Okta 令牌";
		}
		if (text.StartsWith("otpauth://"))
		{
			return "otpauth 链接";
		}
		return "文本/Base32";
	}
}
